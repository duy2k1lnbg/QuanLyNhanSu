-- ==============================================================================
-- Migration V1_14: Overtime Request Workflow Hardening & Actual Overtime Reconciliation
-- Version: V1_14
-- Database: Oracle 21c (HR schema)
-- Description:
--   1. Chuẩn hóa bảng TB_YEUCAU_TANGCA (Overtime Request):
--      - Ràng buộc CHECK cho GIOTANGCA > 0 và <= 24.
--      - Ràng buộc CHECK cho TRANGTHAI: PENDING, APPROVED, REJECTED, CANCELLED.
--      - Ràng buộc Foreign Key NGUOIDUYET -> TB_SYS_USER(IDUSER).
--      - Ràng buộc Foreign Key IDCA -> TB_LOAICA(IDLOAICA).
--      - Index tối ưu hóa truy vấn duyệt & lịch sử.
--      - Unique Index chống tạo request trùng lặp cho các đề xuất đang PENDING/APPROVED.
--   2. Bổ sung liên kết giữa Actual Overtime (TB_TANGCA) và Overtime Request (TB_YEUCAU_TANGCA):
--      - Thêm cột OT_REQUEST_ID vào TB_TANGCA.
--      - Tạo FK_TANGCA_OT_REQUEST trỏ về TB_YEUCAU_TANGCA(ID).
--   3. Đồng bộ hóa ràng buộc chuẩn cho TB_YEUCAU_NGHIPHEP và TB_YEUCAU_DIEUCHINHCONG.
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR CONTINUE;

--------------------------------------------------------------------------------
-- 1. TIỀN XỬ LÝ VÀ CHUẨN HÓA DỮ LIỆU CŨ (PRE-MIGRATION DATA SANITIZATION)
--------------------------------------------------------------------------------

-- Chuẩn hóa trạng thái trong TB_YEUCAU_TANGCA
UPDATE HR.TB_YEUCAU_TANGCA 
SET TRANGTHAI = 'PENDING' 
WHERE TRANGTHAI IS NULL OR TRANGTHAI NOT IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED');

-- Chuẩn hóa số giờ tăng ca không hợp lệ (nếu có)
UPDATE HR.TB_YEUCAU_TANGCA 
SET GIOTANGCA = 1.0 
WHERE GIOTANGCA IS NULL OR GIOTANGCA <= 0;

-- Làm sạch NGUOIDUYET không hợp lệ trước khi tạo Foreign Key
UPDATE HR.TB_YEUCAU_TANGCA 
SET NGUOIDUYET = NULL 
WHERE NGUOIDUYET IS NOT NULL 
  AND NGUOIDUYET NOT IN (SELECT IDUSER FROM HR.TB_SYS_USER);

-- Gán mặc định IDCA = 1 nếu IDCA không tồn tại trong TB_LOAICA
UPDATE HR.TB_YEUCAU_TANGCA 
SET IDCA = 1 
WHERE IDCA IS NOT NULL 
  AND IDCA NOT IN (SELECT IDLOAICA FROM HR.TB_LOAICA);

-- Xử lý an toàn các bản ghi trùng lặp PENDING/APPROVED cũ nếu có trong lịch sử trước khi tạo Unique Index
UPDATE HR.TB_YEUCAU_TANGCA t1
SET t1.TRANGTHAI = 'CANCELLED',
    t1.GHICHUDUYET = 'Tự động hủy bản ghi trùng lặp cũ khi chuẩn hóa hệ thống'
WHERE t1.TRANGTHAI IN ('PENDING', 'APPROVED')
  AND EXISTS (
      SELECT 1 FROM HR.TB_YEUCAU_TANGCA t2
      WHERE t2.MANV = t1.MANV
        AND TRUNC(t2.NGAY) = TRUNC(t1.NGAY)
        AND NVL(t2.IDCA, 1) = NVL(t1.IDCA, 1)
        AND t2.TRANGTHAI IN ('PENDING', 'APPROVED')
        AND t2.ID > t1.ID
  );

-- Chuẩn hóa dữ liệu TB_YEUCAU_NGHIPHEP
UPDATE HR.TB_YEUCAU_NGHIPHEP 
SET TRANGTHAI = 'PENDING' 
WHERE TRANGTHAI IS NULL OR TRANGTHAI NOT IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED');

UPDATE HR.TB_YEUCAU_NGHIPHEP 
SET SONGAY = 1.0 
WHERE SONGAY IS NULL OR SONGAY <= 0;

UPDATE HR.TB_YEUCAU_NGHIPHEP 
SET NGUOIDUYET = NULL 
WHERE NGUOIDUYET IS NOT NULL 
  AND NGUOIDUYET NOT IN (SELECT IDUSER FROM HR.TB_SYS_USER);

-- Chuẩn hóa dữ liệu TB_YEUCAU_DIEUCHINHCONG
UPDATE HR.TB_YEUCAU_DIEUCHINHCONG 
SET TRANGTHAI = 'PENDING' 
WHERE TRANGTHAI IS NULL OR TRANGTHAI NOT IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED');

UPDATE HR.TB_YEUCAU_DIEUCHINHCONG 
SET NGUOIDUYET = NULL 
WHERE NGUOIDUYET IS NOT NULL 
  AND NGUOIDUYET NOT IN (SELECT IDUSER FROM HR.TB_SYS_USER);

COMMIT;

--------------------------------------------------------------------------------
-- 2. BỔ SUNG CỘT OT_REQUEST_ID VÀO TB_TANGCA (ACTUAL OVERTIME)
--------------------------------------------------------------------------------
DECLARE
    v_col_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_col_count 
    FROM all_tab_cols 
    WHERE owner = 'HR' AND table_name = 'TB_TANGCA' AND column_name = 'OT_REQUEST_ID';
    
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_TANGCA ADD (OT_REQUEST_ID NUMBER(22))';
    END IF;
END;
/

-- Tạo Foreign Key từ TB_TANGCA đến TB_YEUCAU_TANGCA
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'FK_TANGCA_OT_REQUEST';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_TANGCA ADD CONSTRAINT FK_TANGCA_OT_REQUEST FOREIGN KEY (OT_REQUEST_ID) REFERENCES HR.TB_YEUCAU_TANGCA(ID) ON DELETE SET NULL';
    END IF;
END;
/

-- Tạo Index cho OT_REQUEST_ID trong TB_TANGCA
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_indexes 
    WHERE owner = 'HR' AND index_name = 'IX_TANGCA_OT_REQ';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE INDEX HR.IX_TANGCA_OT_REQ ON HR.TB_TANGCA(OT_REQUEST_ID)';
    END IF;
END;
/

--------------------------------------------------------------------------------
-- 3. NÂNG CẤP RÀNG BUỘC & KHÓA NGOẠI TRÊN TB_YEUCAU_TANGCA
--------------------------------------------------------------------------------

-- 3.1 CHECK Constraint cho GIOTANGCA
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'CK_YEUCAU_TC_HOURS';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_TANGCA ADD CONSTRAINT CK_YEUCAU_TC_HOURS CHECK (GIOTANGCA > 0 AND GIOTANGCA <= 24)';
    END IF;
END;
/

-- 3.2 CHECK Constraint cho TRANGTHAI
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'CK_YEUCAU_TC_STATUS';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_TANGCA ADD CONSTRAINT CK_YEUCAU_TC_STATUS CHECK (TRANGTHAI IN (''PENDING'', ''APPROVED'', ''REJECTED'', ''CANCELLED''))';
    END IF;
END;
/

-- 3.3 Foreign Key NGUOIDUYET -> TB_SYS_USER(IDUSER)
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'FK_YEUCAU_TC_APPROVER';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_TANGCA ADD CONSTRAINT FK_YEUCAU_TC_APPROVER FOREIGN KEY (NGUOIDUYET) REFERENCES HR.TB_SYS_USER(IDUSER) ON DELETE SET NULL';
    END IF;
END;
/

-- 3.4 Foreign Key IDCA -> TB_LOAICA(IDLOAICA)
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'FK_YEUCAU_TC_LOAICA';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_TANGCA ADD CONSTRAINT FK_YEUCAU_TC_LOAICA FOREIGN KEY (IDCA) REFERENCES HR.TB_LOAICA(IDLOAICA) ON DELETE SET NULL';
    END IF;
END;
/

-- 3.5 Unique Index chống tạo request trùng lặp (Active request: PENDING hoặc APPROVED)
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_indexes 
    WHERE owner = 'HR' AND index_name = 'UQ_YEUCAU_TC_ACTIVE';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE UNIQUE INDEX HR.UQ_YEUCAU_TC_ACTIVE ON HR.TB_YEUCAU_TANGCA (
            CASE WHEN TRANGTHAI IN (''PENDING'', ''APPROVED'') THEN MANV END,
            CASE WHEN TRANGTHAI IN (''PENDING'', ''APPROVED'') THEN TRUNC(NGAY) END,
            CASE WHEN TRANGTHAI IN (''PENDING'', ''APPROVED'') THEN NVL(IDCA, 1) END
        )';
    END IF;
END;
/

-- 3.6 Indexes tối ưu hiệu năng truy vấn cho TB_YEUCAU_TANGCA
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_indexes 
    WHERE owner = 'HR' AND index_name = 'IX_YEUCAU_TC_STATUS_DATE';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE INDEX HR.IX_YEUCAU_TC_STATUS_DATE ON HR.TB_YEUCAU_TANGCA(TRANGTHAI, CREATED_DATE DESC)';
    END IF;
END;
/

DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_indexes 
    WHERE owner = 'HR' AND index_name = 'IX_YEUCAU_TC_APPROVER';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE INDEX HR.IX_YEUCAU_TC_APPROVER ON HR.TB_YEUCAU_TANGCA(NGUOIDUYET)';
    END IF;
END;
/

DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_indexes 
    WHERE owner = 'HR' AND index_name = 'IX_YEUCAU_TC_IDCA';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE INDEX HR.IX_YEUCAU_TC_IDCA ON HR.TB_YEUCAU_TANGCA(IDCA)';
    END IF;
END;
/

--------------------------------------------------------------------------------
-- 4. ĐỒNG BỘ CHUẨN HÓA CHO TB_YEUCAU_NGHIPHEP VÀ TB_YEUCAU_DIEUCHINHCONG
--------------------------------------------------------------------------------

-- 4.1 TB_YEUCAU_NGHIPHEP: CHECK trạng thái & số ngày
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'CK_YEUCAU_NP_STATUS';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_NGHIPHEP ADD CONSTRAINT CK_YEUCAU_NP_STATUS CHECK (TRANGTHAI IN (''PENDING'', ''APPROVED'', ''REJECTED'', ''CANCELLED''))';
    END IF;
END;
/

DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'CK_YEUCAU_NP_DAYS';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_NGHIPHEP ADD CONSTRAINT CK_YEUCAU_NP_DAYS CHECK (SONGAY > 0)';
    END IF;
END;
/

-- 4.2 TB_YEUCAU_NGHIPHEP: FK NGUOIDUYET
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'FK_YEUCAU_NP_APPROVER';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_NGHIPHEP ADD CONSTRAINT FK_YEUCAU_NP_APPROVER FOREIGN KEY (NGUOIDUYET) REFERENCES HR.TB_SYS_USER(IDUSER) ON DELETE SET NULL';
    END IF;
END;
/

-- 4.3 TB_YEUCAU_DIEUCHINHCONG: CHECK trạng thái & FK NGUOIDUYET
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'CK_YEUCAU_DC_STATUS';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_DIEUCHINHCONG ADD CONSTRAINT CK_YEUCAU_DC_STATUS CHECK (TRANGTHAI IN (''PENDING'', ''APPROVED'', ''REJECTED'', ''CANCELLED''))';
    END IF;
END;
/

DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt 
    FROM all_constraints 
    WHERE owner = 'HR' AND constraint_name = 'FK_YEUCAU_DC_APPROVER';
    
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_YEUCAU_DIEUCHINHCONG ADD CONSTRAINT FK_YEUCAU_DC_APPROVER FOREIGN KEY (NGUOIDUYET) REFERENCES HR.TB_SYS_USER(IDUSER) ON DELETE SET NULL';
    END IF;
END;
/

COMMIT;
