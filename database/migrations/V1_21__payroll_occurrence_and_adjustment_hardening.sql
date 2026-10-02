-- ==========================================================================================
-- Migration V1_21: Payroll Occurrence Lifecycle, Disciplinary Deductions & Adjustment Audit
-- (Hardened & Idempotent Version)
-- ==========================================================================================
-- Description:
-- 1. Bổ sung các cột trạng thái nghiệp vụ và vết phê duyệt tường minh cho bảng TB_KHENTHUONG_KYLUAT.
-- 2. Kiểm tra độc lập từng cột (partial schema resilient), tránh lỗi khi script bị ngắt quãng giữa chừng.
-- 3. Hỗ trợ 4 trạng thái: DRAFT (Nháp), APPROVED (Đã duyệt), REVOKED (Thu hồi), PENDING_AUDIT (Chờ đối soát).
-- 4. Chuyển đổi dữ liệu cũ có mốc cố định (cut-off timestamp), chạy lại (re-run) tuyệt đối không biến
--    các bản nháp mới tạo thành PENDING_AUDIT.
-- 5. Replay đầy đủ các hành động kiểm toán (DUYET_PHATSINH, THU_HOI_PHATSINH, SUA_PHATSINH_NHAP, THEM_PHATSINH_NHAP).
--
-- Hướng dẫn vận hành:
-- 1. YÊU CẦU: KHÔNG CHẠY TRỰC TIẾP TRÊN DATABASE PRODUCTION TRONG PHIÊN LÀM VIỆC NÀY.
--    Chỉ chạy trên môi trường kiểm thử (Staging/Sandbox) sau khi sao lưu CSDL schema HR.
-- 2. Không chạy lại lương các kỳ đã khóa.
-- 3. Có kèm câu lệnh Preflight kiểm tra trước khi chạy và Post-check đối soát sau khi chạy.
-- ==========================================================================================

-- PREFLIGHT CHECK:
-- SELECT 'TOTAL_KTKL', COUNT(*) FROM TB_KHENTHUONG_KYLUAT;
-- SELECT 'TOTAL_LOGS', COUNT(*) FROM TB_SYS_LOG WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT';

BEGIN
    IF SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA') <> 'HR' THEN
        RAISE_APPLICATION_ERROR(-20000, 'Yêu cầu chạy script dưới quyền user HR.');
    END IF;
END;
/

-- 1. Bổ sung từng cột trạng thái và kiểm toán độc lập (Partial schema resilience)
DECLARE
    PROCEDURE add_col_if_missing(p_col_name VARCHAR2, p_col_type VARCHAR2) IS
        v_cnt NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cnt FROM USER_TAB_COLUMNS 
        WHERE TABLE_NAME = 'TB_KHENTHUONG_KYLUAT' AND COLUMN_NAME = UPPER(p_col_name);
        
        IF v_cnt = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE TB_KHENTHUONG_KYLUAT ADD (' || p_col_name || ' ' || p_col_type || ')';
            DBMS_OUTPUT.PUT_LINE('Đã thêm cột ' || p_col_name);
        ELSE
            DBMS_OUTPUT.PUT_LINE('Cột ' || p_col_name || ' đã tồn tại.');
        END IF;
    END;
BEGIN
    add_col_if_missing('TRANG_THAI', 'VARCHAR2(30) DEFAULT ''DRAFT'' NOT NULL');
    add_col_if_missing('APPROVED_BY', 'NUMBER NULL');
    add_col_if_missing('APPROVED_DATE', 'TIMESTAMP NULL');
    add_col_if_missing('REVOKED_BY', 'NUMBER NULL');
    add_col_if_missing('REVOKED_DATE', 'TIMESTAMP NULL');
    add_col_if_missing('REVOKED_REASON', 'NVARCHAR2(500) NULL');
    add_col_if_missing('DATA_VERSION', 'NUMBER DEFAULT 1 NOT NULL');
END;
/

-- 2. Thêm ràng buộc kiểm tra hợp lệ cho trạng thái
DECLARE
    v_chk_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_chk_count FROM USER_CONSTRAINTS 
    WHERE CONSTRAINT_NAME = 'CK21_KTKL_TRANG_THAI';
    
    IF v_chk_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_KHENTHUONG_KYLUAT ADD CONSTRAINT CK21_KTKL_TRANG_THAI 
            CHECK (TRANG_THAI IN (''DRAFT'', ''APPROVED'', ''REVOKED'', ''PENDING_AUDIT''))';
        DBMS_OUTPUT.PUT_LINE('Đã tạo ràng buộc CK21_KTKL_TRANG_THAI.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Ràng buộc CK21_KTKL_TRANG_THAI đã tồn tại.');
    END IF;
END;
/

-- 3. Tạo chỉ mục tối ưu truy vấn kỳ và trạng thái
DECLARE
    v_idx_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_idx_count FROM USER_INDEXES 
    WHERE INDEX_NAME = 'IX21_KTKL_PERIOD_STATUS';
    
    IF v_idx_count = 0 THEN
        EXECUTE IMMEDIATE 'CREATE INDEX IX21_KTKL_PERIOD_STATUS 
            ON TB_KHENTHUONG_KYLUAT (NAM_APDUNG, THANG_APDUNG, TRANG_THAI, DELETED_DATE)';
        DBMS_OUTPUT.PUT_LINE('Đã tạo chỉ mục IX21_KTKL_PERIOD_STATUS.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('Chỉ mục IX21_KTKL_PERIOD_STATUS đã tồn tại.');
    END IF;
END;
/

-- 4. Chuyển đổi dữ liệu cũ có kiểm soát (Idempotent Legacy Migration Strategy)
DECLARE
    -- Mốc cố định phân định dữ liệu lịch sử và dữ liệu mới
    c_legacy_cutoff CONSTANT TIMESTAMP := TO_TIMESTAMP('2026-10-01 00:00:00', 'YYYY-MM-DD HH24:MI:SS');
    v_pending_cnt NUMBER := 0;
    v_approved_cnt NUMBER := 0;
    v_revoked_cnt NUMBER := 0;
BEGIN
    -- Bước 4.1: Chỉ chuyển các bản ghi lịch sử tạo TRƯỚC mốc cutoff mà chưa có nhật ký phê duyệt sang PENDING_AUDIT
    -- Tuyệt đối không chuyển các bản nháp mới tạo sau mốc cutoff hoặc bản nháp đang chỉnh sửa
    UPDATE TB_KHENTHUONG_KYLUAT
    SET TRANG_THAI = 'PENDING_AUDIT'
    WHERE TRANG_THAI = 'DRAFT'
      AND (CREATED_DATE IS NULL OR CREATED_DATE <= c_legacy_cutoff)
      AND NVL(DATA_VERSION, 1) = 1
      AND SOQUYETDINH NOT IN (
          SELECT DISTINCT ID_BAN_GHI FROM TB_SYS_LOG 
          WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
      );
    v_pending_cnt := SQL%ROWCOUNT;
    DBMS_OUTPUT.PUT_LINE('Chuyển ' || v_pending_cnt || ' bản ghi lịch sử thiếu vết sang PENDING_AUDIT.');

    -- Bước 4.2: Replay nhật ký kiểm toán hệ thống để xác định trạng thái thực tế
    -- Bao gồm cả DUYET_PHATSINH, THU_HOI_PHATSINH, SUA_PHATSINH_NHAP, THEM_PHATSINH_NHAP
    -- 4.2.A: Cập nhật APPROVED đối với các chứng từ có hành động cuối cùng là DUYET_PHATSINH
    UPDATE TB_KHENTHUONG_KYLUAT k
    SET TRANG_THAI = 'APPROVED',
        APPROVED_BY = (
            SELECT MANV_THUCHIEN FROM (
                SELECT ID_BAN_GHI, MANV_THUCHIEN,
                       ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                FROM TB_SYS_LOG
                WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                  AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'SUA_PHATSINH_NHAP', 'THEM_PHATSINH_NHAP')
            ) sl WHERE sl.rn = 1 AND sl.ID_BAN_GHI = k.SOQUYETDINH
        ),
        APPROVED_DATE = (
            SELECT THOIGIAN FROM (
                SELECT ID_BAN_GHI, THOIGIAN,
                       ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                FROM TB_SYS_LOG
                WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                  AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'SUA_PHATSINH_NHAP', 'THEM_PHATSINH_NHAP')
            ) sl WHERE sl.rn = 1 AND sl.ID_BAN_GHI = k.SOQUYETDINH
        )
    WHERE SOQUYETDINH IN (
        SELECT ID_BAN_GHI FROM (
            SELECT ID_BAN_GHI, HANHDONG,
                   ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
            FROM TB_SYS_LOG
            WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
              AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'SUA_PHATSINH_NHAP', 'THEM_PHATSINH_NHAP')
        )
        WHERE rn = 1 AND HANHDONG = 'DUYET_PHATSINH'
    )
    AND TRANG_THAI <> 'APPROVED';
    v_approved_cnt := SQL%ROWCOUNT;
    DBMS_OUTPUT.PUT_LINE('Replay phê duyệt ' || v_approved_cnt || ' chứng từ sang APPROVED.');

    -- 4.2.B: Cập nhật REVOKED đối với các chứng từ có hành động cuối cùng là THU_HOI_PHATSINH
    UPDATE TB_KHENTHUONG_KYLUAT k
    SET TRANG_THAI = 'REVOKED',
        REVOKED_BY = (
            SELECT MANV_THUCHIEN FROM (
                SELECT ID_BAN_GHI, MANV_THUCHIEN,
                       ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                FROM TB_SYS_LOG
                WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                  AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'SUA_PHATSINH_NHAP', 'THEM_PHATSINH_NHAP')
            ) sl WHERE sl.rn = 1 AND sl.ID_BAN_GHI = k.SOQUYETDINH
        ),
        REVOKED_DATE = (
            SELECT THOIGIAN FROM (
                SELECT ID_BAN_GHI, THOIGIAN,
                       ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
                FROM TB_SYS_LOG
                WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
                  AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'SUA_PHATSINH_NHAP', 'THEM_PHATSINH_NHAP')
            ) sl WHERE sl.rn = 1 AND sl.ID_BAN_GHI = k.SOQUYETDINH
        ),
        REVOKED_REASON = NVL(REVOKED_REASON, 'Thu hồi theo nhật ký kiểm toán hệ thống')
    WHERE SOQUYETDINH IN (
        SELECT ID_BAN_GHI FROM (
            SELECT ID_BAN_GHI, HANHDONG,
                   ROW_NUMBER() OVER (PARTITION BY ID_BAN_GHI ORDER BY THOIGIAN DESC, ID_LOG DESC) as rn
            FROM TB_SYS_LOG
            WHERE TEN_BANG = 'TB_KHENTHUONG_KYLUAT'
              AND HANHDONG IN ('DUYET_PHATSINH', 'THU_HOI_PHATSINH', 'SUA_PHATSINH_NHAP', 'THEM_PHATSINH_NHAP')
        )
        WHERE rn = 1 AND HANHDONG = 'THU_HOI_PHATSINH'
    )
    AND TRANG_THAI <> 'REVOKED';
    v_revoked_cnt := SQL%ROWCOUNT;
    DBMS_OUTPUT.PUT_LINE('Replay thu hồi ' || v_revoked_cnt || ' chứng từ sang REVOKED.');

    COMMIT;
END;
/

COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.TRANG_THAI IS 'Trạng thái phát sinh lương: DRAFT (Nháp), APPROVED (Đã duyệt), REVOKED (Thu hồi), PENDING_AUDIT (Chờ đối soát)';
COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.APPROVED_BY IS 'Người dùng thực hiện phê duyệt phát sinh lương';
COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.APPROVED_DATE IS 'Thời điểm phê duyệt phát sinh lương';
COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.REVOKED_BY IS 'Người dùng thực hiện thu hồi phê duyệt phát sinh lương';
COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.REVOKED_DATE IS 'Thời điểm thu hồi phê duyệt phát sinh lương';
COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.REVOKED_REASON IS 'Lý do thu hồi phê duyệt khoản phát sinh lương';
COMMENT ON COLUMN TB_KHENTHUONG_KYLUAT.DATA_VERSION IS 'Phiên bản dữ liệu kiểm soát đồng thời';

-- ==========================================================================================
-- POST-MIGRATION RECONCILIATION & VALIDATION QUERIES:
-- ==========================================================================================
-- 1. Thống kê số lượng theo trạng thái:
-- SELECT TRANG_THAI, COUNT(*) FROM TB_KHENTHUONG_KYLUAT GROUP BY TRANG_THAI;
--
-- 2. Kiểm tra tính toàn vẹn vết duyệt (APPROVED phải có người duyệt):
-- SELECT SOQUYETDINH, TRANG_THAI, APPROVED_BY, APPROVED_DATE
-- FROM TB_KHENTHUONG_KYLUAT WHERE TRANG_THAI = 'APPROVED' AND APPROVED_BY IS NULL;
--
-- 3. Kiểm tra tính toàn vẹn vết thu hồi (REVOKED phải có lý do):
-- SELECT SOQUYETDINH, TRANG_THAI, REVOKED_BY, REVOKED_REASON
-- FROM TB_KHENTHUONG_KYLUAT WHERE TRANG_THAI = 'REVOKED' AND REVOKED_REASON IS NULL;
--
-- ==========================================================================================
-- ROLLBACK SCRIPT (DÙNG TRONG TRƯỜNG HỢP CẦN PHỤC HỒI SCHEMA):
-- ==========================================================================================
-- ALTER TABLE TB_KHENTHUONG_KYLUAT DROP CONSTRAINT CK21_KTKL_TRANG_THAI;
-- DROP INDEX IX21_KTKL_PERIOD_STATUS;
-- ALTER TABLE TB_KHENTHUONG_KYLUAT DROP (
--     TRANG_THAI, APPROVED_BY, APPROVED_DATE, REVOKED_BY, REVOKED_DATE, REVOKED_REASON, DATA_VERSION
-- );
-- ==========================================================================================
