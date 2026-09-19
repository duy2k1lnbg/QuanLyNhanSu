-- ==============================================================================
-- Rollback Migration V1_12: Hoàn tác liên kết User - Nhân viên và phân quyền Mobile
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR CONTINUE;

-- 1. Xóa quyền Mobile khỏi TB_SYS_RIGHT
DELETE FROM TB_SYS_RIGHT WHERE FUNCTION_CODE LIKE 'MOBILE_%';

-- 2. Xóa chức năng Mobile khỏi TB_SYS_FUNCTION
DELETE FROM TB_SYS_FUNCTION WHERE FUNCTION_CODE LIKE 'MOBILE_%';

-- 3. Xóa ràng buộc Khóa Ngoại và Ràng Buộc Duy Nhất
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER DROP CONSTRAINT FK_SYS_USER_NV';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'DROP INDEX UQ_SYS_USER_MANV';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- 4. Xóa các cột MANV và CLIENT_TYPE khỏi TB_SYS_USER
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER DROP (MANV, CLIENT_TYPE)';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

COMMIT;
