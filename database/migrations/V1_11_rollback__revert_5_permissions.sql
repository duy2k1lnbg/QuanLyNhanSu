-- ==============================================================================
-- Rollback V1_11: Hoàn Tác Nâng Cấp Phân Quyền 5 Quyền
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR CONTINUE;

-- 1. Khôi phục lại trigger INSERT_FUNC cũ
CREATE OR REPLACE TRIGGER INSERT_FUNC
AFTER INSERT ON TB_SYS_FUNCTION
REFERENCING OLD AS OLD NEW AS NEW
BEGIN
  INSERT INTO tb_sys_right(function_code, iduser, user_right)
  SELECT A.function_code, B.iduser,0
  FROM tb_sys_function A, tb_sys_user B
  WHERE a.function_code NOT IN (SELECT DISTINCT function_code FROM tb_sys_right);
END;
/

-- 2. Khôi phục lại trigger USER_INSERT cũ
CREATE OR REPLACE TRIGGER USER_INSERT
AFTER INSERT ON TB_SYS_USER
BEGIN
  INSERT INTO tb_sys_right(function_code, iduser, user_right)
  SELECT A.function_code, B.iduser,0
  FROM tb_sys_function A, tb_sys_user B
  WHERE b.iduser = (SELECT MAX(iduser)FROM tb_sys_user);

  INSERT INTO tb_sys_right_report(rep_code, iduser,user_right)
  SELECT A.rep_code, B.iduser,0
  FROM tb_sys_report A, tb_sys_user B
  WHERE B.iduser = (SELECT MAX(iduser)FROM tb_sys_user);
END;
/

-- 3. Xóa các ràng buộc Check Constraint
ALTER TABLE TB_SYS_RIGHT DROP CONSTRAINT CK_SYS_RIGHT_VIEW;
ALTER TABLE TB_SYS_RIGHT DROP CONSTRAINT CK_SYS_RIGHT_ADD;
ALTER TABLE TB_SYS_RIGHT DROP CONSTRAINT CK_SYS_RIGHT_EDIT;
ALTER TABLE TB_SYS_RIGHT DROP CONSTRAINT CK_SYS_RIGHT_DELETE;
ALTER TABLE TB_SYS_RIGHT DROP CONSTRAINT CK_SYS_RIGHT_PRINT;

-- 4. Xóa 5 cột quyền mới
ALTER TABLE TB_SYS_RIGHT DROP (CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT);

-- 5. Khôi phục dữ liệu từ bảng backup nếu cần
-- DELETE FROM TB_SYS_RIGHT;
-- INSERT INTO TB_SYS_RIGHT SELECT * FROM TB_SYS_RIGHT_BACKUP;
-- COMMIT;

EXIT;
