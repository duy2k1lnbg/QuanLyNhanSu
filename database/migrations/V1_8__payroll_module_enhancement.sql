-- ==============================================================================
-- Migration V1_8: Hoàn thiện Module Tính Lương (Payroll) & Tăng Ca
-- ==============================================================================

-- 1. Bổ sung các cột cho TB_BANGLUONG
DECLARE
    PROCEDURE add_col_if_not_exists(p_table VARCHAR2, p_col VARCHAR2, p_type VARCHAR2) IS
        v_cnt NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cnt FROM USER_TAB_COLUMNS 
        WHERE TABLE_NAME = UPPER(p_table) AND COLUMN_NAME = UPPER(p_col);
        IF v_cnt = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE ' || p_table || ' ADD (' || p_col || ' ' || p_type || ')';
        END IF;
    END;
BEGIN
    add_col_if_not_exists('TB_BANGLUONG', 'CONG_LAMNGAY', 'NUMBER(4,1)');
    add_col_if_not_exists('TB_BANGLUONG', 'TONG_CONG', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'LUONG_BHXH', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'TIEN_BHXH', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'TIEN_BHYT', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'TIEN_BHTN', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'TIEN_CONG_DOAN', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'THUE_TNCN', 'NUMBER(15,2)');
    add_col_if_not_exists('TB_BANGLUONG', 'HOAN_THUE', 'NUMBER(15,2)');
    
    -- 2. Bổ sung IDLOAITANGCA cho TB_TANGCA
    add_col_if_not_exists('TB_TANGCA', 'IDLOAITANGCA', 'NUMBER(38)');
END;
/

-- 3. Cập nhật hệ số ca đêm trong TB_LOAICA lên 1.30 theo bảng lương thực tế
UPDATE TB_LOAICA SET HESOLOAICA = 1.30 WHERE IDLOAICA = 2;
COMMIT;
/

-- 4. Tạo khóa ngoại FK_TANGCA_LOAIOT nếu chưa tồn tại
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM USER_CONSTRAINTS WHERE CONSTRAINT_NAME = 'FK_TANGCA_LOAIOT';
    IF v_cnt = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'ALTER TABLE TB_TANGCA ADD CONSTRAINT FK_TANGCA_LOAIOT FOREIGN KEY (IDLOAITANGCA) REFERENCES TB_HESO_TANGCA(ID)';
        EXCEPTION
            WHEN OTHERS THEN
                NULL;
        END;
    END IF;
END;
/
COMMIT;
EXIT;
