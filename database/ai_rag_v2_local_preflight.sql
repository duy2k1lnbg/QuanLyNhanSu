-- Read-only local preflight. No DDL, DML, GRANT or REVOKE.
-- Connect using the same HR service as the local API. Do not put credentials into a command/log.
WHENEVER SQLERROR EXIT FAILURE ROLLBACK
SET PAGESIZE 200
SET LINESIZE 180
SELECT SYS_CONTEXT('USERENV','SESSION_USER') AS SESSION_USER FROM DUAL;
SELECT OWNER,OBJECT_NAME,OBJECT_TYPE,STATUS FROM ALL_OBJECTS WHERE OWNER='HR' AND (OBJECT_NAME LIKE 'TB_AI_%' OR OBJECT_NAME LIKE 'PKG_AI_%' OR OBJECT_NAME LIKE 'V_AI_%') ORDER BY OBJECT_TYPE,OBJECT_NAME;
SELECT NAMESPACE,SCHEMA,PACKAGE FROM ALL_CONTEXT WHERE NAMESPACE='HRMS_AI_CTX';
SELECT TABLE_SCHEMA,TABLE_NAME,PRIVILEGE FROM ALL_TAB_PRIVS WHERE GRANTEE='AI_READONLY' AND TABLE_SCHEMA='HR' ORDER BY TABLE_NAME,PRIVILEGE;
-- This block stops BEFORE an AI migration can create partial objects on an older source schema.
-- Verify source contract BEFORE any DDL: Oracle DDL commits implicitly.
DECLARE
  v_count NUMBER;
  PROCEDURE need_column(p_table VARCHAR2,p_column VARCHAR2) IS
  BEGIN
    SELECT COUNT(*) INTO v_count FROM ALL_TAB_COLUMNS WHERE OWNER='HR' AND TABLE_NAME=p_table AND COLUMN_NAME=p_column;
    IF v_count<>1 THEN RAISE_APPLICATION_ERROR(-20030,'AI prerequisite missing: HR.'||p_table||'.'||p_column||'; review V1_16/V1_18/V1_20 before AI rollout'); END IF;
  END;
BEGIN
  need_column('TB_BANGLUONG','CONG_THUCTE');
  need_column('TB_BANGLUONG','IDBL');
  need_column('TB_BANGLUONG','LUONG_CONG_THUCTE');
  need_column('TB_BANGLUONG','MAKYCONG');
  need_column('TB_BANGLUONG','MANV');
  need_column('TB_BANGLUONG','NAM');
  need_column('TB_BANGLUONG','RUN_ID');
  need_column('TB_BANGLUONG','THANG');
  need_column('TB_BANGLUONG','THUC_LINH');
  need_column('TB_BANGLUONG','TRANGTHAI_CHITRA');
  need_column('TB_BAOHIEM','IDBH');
  need_column('TB_BAOHIEM','MANV');
  need_column('TB_BAOHIEM','NGAYCAP');
  need_column('TB_BAOHIEM','NOICAP');
  need_column('TB_BAOHIEM','NOIKHAMBENH');
  need_column('TB_BAOHIEM','SOBH');
  need_column('TB_CHUCVU','IDCV');
  need_column('TB_CHUCVU','TENCV');
  need_column('TB_HOPDONG','DEL_DATE');
  need_column('TB_HOPDONG','HESOLUONG');
  need_column('TB_HOPDONG','LANKY');
  need_column('TB_HOPDONG','MANV');
  need_column('TB_HOPDONG','NGAYBATDAU');
  need_column('TB_HOPDONG','NGAYKETTHUC');
  need_column('TB_HOPDONG','NGAYKY');
  need_column('TB_HOPDONG','SOHD');
  need_column('TB_KYCONG','CONG_PUBLISH_REV');
  need_column('TB_KYCONG','DELETED_DATE');
  need_column('TB_KYCONG','KHOA');
  need_column('TB_KYCONG','MAKYCONG');
  need_column('TB_KYCONG','NAM');
  need_column('TB_KYCONG','THANG');
  need_column('TB_KYCONG','TRANGTHAI');
  need_column('TB_KYCONGCHITIET','CONGCHUNHAT');
  need_column('TB_KYCONGCHITIET','CONGNGAYLE');
  need_column('TB_KYCONGCHITIET','MAKYCONG');
  need_column('TB_KYCONGCHITIET','MANV');
  need_column('TB_KYCONGCHITIET','NGAYPHEP');
  need_column('TB_KYCONGCHITIET','NGHIKHONGPHEP');
  need_column('TB_KYCONGCHITIET','TONGNGAYCONG');
  need_column('TB_NANGLUONG_NHANVIEN','DELETED_DATE');
  need_column('TB_NANGLUONG_NHANVIEN','HESOLUONG_NEW');
  need_column('TB_NANGLUONG_NHANVIEN','HESOLUONG_NOW');
  need_column('TB_NANGLUONG_NHANVIEN','MANV');
  need_column('TB_NANGLUONG_NHANVIEN','NGAYKYNL');
  need_column('TB_NANGLUONG_NHANVIEN','NGAYLENLUONG');
  need_column('TB_NANGLUONG_NHANVIEN','SOHD');
  need_column('TB_NANGLUONG_NHANVIEN','SOQDNL');
  need_column('TB_NHANVIEN','DATHOIVIEC');
  need_column('TB_NHANVIEN','DELETED_DATE');
  need_column('TB_NHANVIEN','DIACHI');
  need_column('TB_NHANVIEN','DIENTHOAI');
  need_column('TB_NHANVIEN','HOTEN');
  need_column('TB_NHANVIEN','IDCTY');
  need_column('TB_NHANVIEN','IDCV');
  need_column('TB_NHANVIEN','IDPB');
  need_column('TB_NHANVIEN','MANV');
  need_column('TB_NHANVIEN','NGAYSINH');
  need_column('TB_NHANVIEN_PHUCAP','DELETED_DATE');
  need_column('TB_NHANVIEN_PHUCAP','DEN_NGAY');
  need_column('TB_NHANVIEN_PHUCAP','IDPC');
  need_column('TB_NHANVIEN_PHUCAP','MANV');
  need_column('TB_NHANVIEN_PHUCAP','SOTIEN');
  need_column('TB_NHANVIEN_PHUCAP','TU_NGAY');
  need_column('TB_PAYROLL_CALCULATION_RUN','FINISHED_AT');
  need_column('TB_PAYROLL_CALCULATION_RUN','MAKYCONG');
  need_column('TB_PAYROLL_CALCULATION_RUN','RUN_ID');
  need_column('TB_PAYROLL_CALCULATION_RUN','STATUS');
  need_column('TB_PHONGBAN','IDPB');
  need_column('TB_PHONGBAN','TENPB');
  need_column('TB_PHUCAP','IDPC');
  need_column('TB_PHUCAP','TENPC');
  need_column('TB_TANGCA','DELETED_DATE');
  need_column('TB_TANGCA','IDTCA');
  need_column('TB_TANGCA','MANV');
  need_column('TB_TANGCA','NAM');
  need_column('TB_TANGCA','NGAY');
  need_column('TB_TANGCA','SOGIO');
  need_column('TB_TANGCA','THANG');
  need_column('TB_UNGLUONG','DELETED_DATE');
  need_column('TB_UNGLUONG','IDUL');
  need_column('TB_UNGLUONG','MANV');
  need_column('TB_UNGLUONG','NAM');
  need_column('TB_UNGLUONG','NGAY');
  need_column('TB_UNGLUONG','SOTIENUNG');
  need_column('TB_UNGLUONG','THANG');
  SELECT COUNT(*) INTO v_count FROM USER_CONSTRAINTS c
   WHERE c.TABLE_NAME='TB_BANGLUONG' AND c.CONSTRAINT_TYPE IN ('P','U') AND c.STATUS='ENABLED'
    AND (SELECT COUNT(*) FROM USER_CONS_COLUMNS x WHERE x.CONSTRAINT_NAME=c.CONSTRAINT_NAME)=2
    AND (SELECT COUNT(*) FROM USER_CONS_COLUMNS x WHERE x.CONSTRAINT_NAME=c.CONSTRAINT_NAME AND x.COLUMN_NAME IN ('MANV','MAKYCONG'))=2;
  IF v_count=0 THEN RAISE_APPLICATION_ERROR(-20031,'AI payroll prerequisite missing: unique MANV/MAKYCONG constraint'); END IF;
END;
/
-- DBA must separately review reader role/system privileges and legacy reader-owned views.
-- Approval is needed before the owner/DBA migration; this preflight makes no database changes.