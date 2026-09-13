-- ====================================================================
-- HRMS Enterprise Database Migration Script
-- Version: V1_5
-- Description: Chuẩn hóa 13 loại phụ cấp theo hợp đồng lao động, 
--              hủy bỏ tự động phát sinh phụ cấp (Drop TRG_AUTO_PHUCAP),
--              chuyển phụ cấp sang quản lý theo Hợp đồng (MAKYCONG = 0).
-- ====================================================================
SET DEFINE OFF;

-- 1. HỦY BỎ TRIGGER TỰ ĐỘNG PHÁT SINH PHỤ CẤP
BEGIN
    EXECUTE IMMEDIATE 'DROP TRIGGER TRG_AUTO_PHUCAP';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -4080 THEN -- ORA-04080: trigger does not exist
            RAISE;
        END IF;
END;
/

-- 2. CẬP NHẬT / TẠO MỚI 13 LOẠI PHỤ CẤP CHUẨN
MERGE INTO TB_PHUCAP dst
USING (
    SELECT 1 AS IDPC,  N'Phụ cấp nhà ở' AS TENPC FROM DUAL UNION ALL
    SELECT 2,          N'Phụ cấp đi lại' FROM DUAL UNION ALL
    SELECT 3,          N'Phụ cấp gia đình' FROM DUAL UNION ALL
    SELECT 4,          N'Phụ cấp người phụ thuộc' FROM DUAL UNION ALL
    SELECT 5,          N'Phụ cấp chức vụ' FROM DUAL UNION ALL
    SELECT 6,          N'Phụ cấp chứng chỉ' FROM DUAL UNION ALL
    SELECT 7,          N'Phụ cấp kỹ năng' FROM DUAL UNION ALL
    SELECT 8,          N'Phụ cấp khu vực' FROM DUAL UNION ALL
    SELECT 9,          N'Phụ cấp chuyên cần' FROM DUAL UNION ALL
    SELECT 10,         N'Phụ cấp thâm niên' FROM DUAL UNION ALL
    SELECT 11,         N'Phụ cấp làm việc tại nhà' FROM DUAL UNION ALL
    SELECT 12,         N'Phụ cấp đặc biệt' FROM DUAL UNION ALL
    SELECT 13,         N'Phụ cấp khác' FROM DUAL
) src
ON (dst.IDPC = src.IDPC)
WHEN MATCHED THEN
    UPDATE SET dst.TENPC = src.TENPC
WHEN NOT MATCHED THEN
    INSERT (IDPC, TENPC) VALUES (src.IDPC, src.TENPC);

-- 3. KHỞI TẠO BỘ PHỤ CẤP HỢP ĐỒNG GỐC (MAKYCONG = 0) CHO TẤT CẢ NHÂN VIÊN
DECLARE
    v_cnt NUMBER;
BEGIN
    FOR r_nv IN (SELECT MANV FROM TB_NHANVIEN) LOOP
        FOR i IN 1..13 LOOP
            SELECT COUNT(*) INTO v_cnt 
            FROM TB_NHANVIEN_PHUCAP 
            WHERE MANV = r_nv.MANV AND IDPC = i AND MAKYCONG = 0;

            IF v_cnt = 0 THEN
                DECLARE
                    v_tien NUMBER := 0;
                BEGIN
                    IF i <= 7 THEN
                        SELECT NVL(MAX(SOTIEN), 0) INTO v_tien
                        FROM TB_NHANVIEN_PHUCAP
                        WHERE MANV = r_nv.MANV AND IDPC = i AND SOTIEN > 0;
                    END IF;

                    INSERT INTO TB_NHANVIEN_PHUCAP (MANV, IDPC, MAKYCONG, SOTIEN, GHICHU, CREATED_DATE)
                    VALUES (r_nv.MANV, i, 0, v_tien, N'Phụ cấp theo hợp đồng', SYSDATE);
                EXCEPTION
                    WHEN OTHERS THEN
                        INSERT INTO TB_NHANVIEN_PHUCAP (MANV, IDPC, MAKYCONG, SOTIEN, GHICHU, CREATED_DATE)
                        VALUES (r_nv.MANV, i, 0, 0, N'Phụ cấp theo hợp đồng', SYSDATE);
                END;
            END IF;
        END LOOP;
    END LOOP;
END;
/

COMMIT;
