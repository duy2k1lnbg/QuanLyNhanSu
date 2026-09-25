-- ==============================================================================================
-- Migration: V1_17__fix_tb_bangcong_foreign_key.sql
-- Description: Khắc phục lỗi schema kế thừa: TB_BANGCONG_NV_FK1 ban đầu bị tạo sai cột
--              (MABC -> TB_NHANVIEN.MANV thay vì MANV -> TB_NHANVIEN.MANV).
--              Điều này dẫn đến lỗi ORA-02291 khi MABC tự tăng vượt quá MANV của bảng nhân viên.
-- ==============================================================================================

DECLARE
    v_fk_col VARCHAR2(100);
BEGIN
    SELECT column_name INTO v_fk_col
    FROM user_cons_columns
    WHERE constraint_name = 'TB_BANGCONG_NV_FK1'
      AND ROWNUM = 1;

    IF v_fk_col = 'MABC' THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_BANGCONG DROP CONSTRAINT TB_BANGCONG_NV_FK1';
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_BANGCONG ADD CONSTRAINT TB_BANGCONG_NV_FK1 FOREIGN KEY (MANV) REFERENCES HR.TB_NHANVIEN(MANV)';
        DBMS_OUTPUT.PUT_LINE('TB_BANGCONG_NV_FK1 successfully updated to reference MANV.');
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        EXECUTE IMMEDIATE 'ALTER TABLE HR.TB_BANGCONG ADD CONSTRAINT TB_BANGCONG_NV_FK1 FOREIGN KEY (MANV) REFERENCES HR.TB_NHANVIEN(MANV)';
        DBMS_OUTPUT.PUT_LINE('TB_BANGCONG_NV_FK1 created with MANV.');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Notice: ' || SQLERRM);
END;
/
