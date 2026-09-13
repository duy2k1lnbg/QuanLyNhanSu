-- ==============================================================================
-- Migration V1_11: Nâng Cấp Hệ Thống Phân Quyền 5 Quyền Cơ Bản
-- Function-level 5 permissions: CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

-- 1. Sao lưu bảng phân quyền cũ
DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM user_tables WHERE table_name = 'TB_SYS_RIGHT_BACKUP';
    IF v_count > 0 THEN
        EXECUTE IMMEDIATE 'DROP TABLE TB_SYS_RIGHT_BACKUP CASCADE CONSTRAINTS';
    END IF;
    EXECUTE IMMEDIATE 'CREATE TABLE TB_SYS_RIGHT_BACKUP AS SELECT * FROM TB_SYS_RIGHT';
END;
/

-- 2. Thêm 5 cột quyền mới vào TB_SYS_RIGHT nếu chưa tồn tại
DECLARE
    v_col_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_col_count 
    FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_RIGHT' AND column_name = 'CAN_VIEW';
    
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_RIGHT ADD (
            CAN_VIEW   NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_ADD    NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_EDIT   NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_DELETE NUMBER(1) DEFAULT 0 NOT NULL,
            CAN_PRINT  NUMBER(1) DEFAULT 0 NOT NULL
        )';
    END IF;
END;
/

-- 3. Di chuyển dữ liệu cũ:
-- Nếu USER_RIGHT = 1 -> cấp đủ cả 5 quyền (Xem, Thêm, Sửa, Xóa, In) để bảo toàn 100% quyền hiện có
UPDATE TB_SYS_RIGHT 
SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1 
WHERE USER_RIGHT = 1;

-- Nếu USER_RIGHT = 0 hoặc NULL -> đặt về 0
UPDATE TB_SYS_RIGHT 
SET CAN_VIEW = 0, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0 
WHERE USER_RIGHT = 0 OR USER_RIGHT IS NULL;

-- Đảm bảo USER_RIGHT luôn đồng bộ với CAN_VIEW
UPDATE TB_SYS_RIGHT SET USER_RIGHT = CAN_VIEW;

COMMIT;

-- 4. Bổ sung các ràng buộc Check Constraint cho 5 quyền
DECLARE
    PROCEDURE add_check(p_constraint_name VARCHAR2, p_check_clause VARCHAR2) IS
        v_chk NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_chk FROM user_constraints WHERE constraint_name = p_constraint_name;
        IF v_chk = 0 THEN
            EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_RIGHT ADD CONSTRAINT ' || p_constraint_name || ' CHECK (' || p_check_clause || ')';
        END IF;
    END;
BEGIN
    add_check('CK_SYS_RIGHT_VIEW',   'CAN_VIEW IN (0, 1)');
    add_check('CK_SYS_RIGHT_ADD',    'CAN_ADD IN (0, 1)');
    add_check('CK_SYS_RIGHT_EDIT',   'CAN_EDIT IN (0, 1)');
    add_check('CK_SYS_RIGHT_DELETE', 'CAN_DELETE IN (0, 1)');
    add_check('CK_SYS_RIGHT_PRINT',  'CAN_PRINT IN (0, 1)');
END;
/

-- 5. Cập nhật Trigger INSERT_FUNC
CREATE OR REPLACE TRIGGER INSERT_FUNC
AFTER INSERT ON TB_SYS_FUNCTION
FOR EACH ROW
BEGIN
    INSERT INTO TB_SYS_RIGHT (FUNCTION_CODE, IDUSER, USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
    SELECT :NEW.FUNCTION_CODE, u.IDUSER, 0, 0, 0, 0, 0, 0
    FROM TB_SYS_USER u
    WHERE NOT EXISTS (
        SELECT 1 FROM TB_SYS_RIGHT r 
        WHERE r.FUNCTION_CODE = :NEW.FUNCTION_CODE AND r.IDUSER = u.IDUSER
    );
END;
/

-- 6. Cập nhật Trigger USER_INSERT
CREATE OR REPLACE TRIGGER USER_INSERT
AFTER INSERT ON TB_SYS_USER
FOR EACH ROW
BEGIN
    INSERT INTO TB_SYS_RIGHT (FUNCTION_CODE, IDUSER, USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
    SELECT f.FUNCTION_CODE, :NEW.IDUSER, 0, 0, 0, 0, 0, 0
    FROM TB_SYS_FUNCTION f
    WHERE NOT EXISTS (
        SELECT 1 FROM TB_SYS_RIGHT r 
        WHERE r.FUNCTION_CODE = f.FUNCTION_CODE AND r.IDUSER = :NEW.IDUSER
    );

    INSERT INTO TB_SYS_RIGHT_REPORT (REP_CODE, IDUSER, USER_RIGHT)
    SELECT r.REP_CODE, :NEW.IDUSER, 0
    FROM TB_SYS_REPORT r
    WHERE NOT EXISTS (
        SELECT 1 FROM TB_SYS_RIGHT_REPORT rr 
        WHERE rr.REP_CODE = r.REP_CODE AND rr.IDUSER = :NEW.IDUSER
    );
END;
/

-- 7. Cấp toàn quyền cho quản trị viên (ADMIN IDUSER = 1) trên tất cả Function
UPDATE TB_SYS_RIGHT 
SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1, USER_RIGHT = 1
WHERE IDUSER = 1;

COMMIT;

-- 8. Kiểm tra kết quả
SELECT COUNT(*) AS TOTAL_RIGHTS, 
       SUM(CAN_VIEW) AS TOTAL_VIEW, 
       SUM(CAN_ADD) AS TOTAL_ADD, 
       SUM(CAN_EDIT) AS TOTAL_EDIT, 
       SUM(CAN_DELETE) AS TOTAL_DELETE, 
       SUM(CAN_PRINT) AS TOTAL_PRINT
FROM TB_SYS_RIGHT;

EXIT;
