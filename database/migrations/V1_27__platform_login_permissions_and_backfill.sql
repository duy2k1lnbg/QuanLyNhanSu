-- ==============================================================================
-- Migration V1_27: Platform Login Permissions & Initial Migration Backfill
-- File: database/migrations/V1_27__platform_login_permissions_and_backfill.sql
-- Description:
--   1. Thêm 3 mã quyền đăng nhập nền tảng vào TB_SYS_FUNCTION:
--      - F_LOGIN_DESKTOP: Quyền tạo và sử dụng phiên Desktop WinForms
--      - F_LOGIN_WEB: Quyền tạo và sử dụng phiên Web Portal
--      - F_LOGIN_MOBILE: Quyền tạo và sử dụng phiên Mobile App (yêu cầu hồ sơ nhân viên)
--   2. Cập nhật / Đảm bảo TB_SYS_RIGHT chỉ lưu CAN_VIEW=1 cho F_LOGIN_*,
--      toàn bộ CAN_ADD/EDIT/DELETE/PRINT luôn bằng 0.
--   3. Backfill dữ liệu ban đầu an toàn từ CLIENT_TYPE cũ mà không mở quyền tùy tiện.
--   4. Ghi nhận dấu mốc cutover vào TB_CONFIG & tạo view TB_SYS_CONFIG.
-- ==============================================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

PROMPT [MIGRATION V1_27] Starting platform login permissions migration...

-- ------------------------------------------------------------------------------
-- 1. Kiểm tra bảng cấu hình hệ thống TB_CONFIG (Nguồn dữ liệu chuẩn)
-- ------------------------------------------------------------------------------
PROMPT [MIGRATION V1_27] Using HR.TB_CONFIG as the authoritative configuration repository.

-- ------------------------------------------------------------------------------
-- 2. Bổ sung 3 Function Codes vào TB_SYS_FUNCTION
-- ------------------------------------------------------------------------------
MERGE INTO TB_SYS_FUNCTION tgt
USING (
    SELECT 'F_LOGIN_DESKTOP' AS FUNCTION_CODE, N'Đăng nhập Desktop' AS DESCRIPTION, 'SYSTEM' AS PARENT, 1 AS SORT FROM DUAL
    UNION ALL
    SELECT 'F_LOGIN_WEB'     AS FUNCTION_CODE, N'Đăng nhập Website' AS DESCRIPTION, 'SYSTEM' AS PARENT, 2 AS SORT FROM DUAL
    UNION ALL
    SELECT 'F_LOGIN_MOBILE'  AS FUNCTION_CODE, N'Đăng nhập Mobile'  AS DESCRIPTION, 'SYSTEM' AS PARENT, 3 AS SORT FROM DUAL
) src
ON (tgt.FUNCTION_CODE = src.FUNCTION_CODE)
WHEN MATCHED THEN
    UPDATE SET tgt.DESCRIPTION = src.DESCRIPTION, tgt.PARENT = src.PARENT, tgt.SORT = src.SORT
WHEN NOT MATCHED THEN
    INSERT (FUNCTION_CODE, DESCRIPTION, PARENT, SORT)
    VALUES (src.FUNCTION_CODE, src.DESCRIPTION, src.PARENT, src.SORT);

COMMIT;
PROMPT [MIGRATION V1_27] Function codes merged successfully.

-- ------------------------------------------------------------------------------
-- 3. Đảm bảo các dòng mặc định trong TB_SYS_RIGHT cho tất cả tài khoản
-- ------------------------------------------------------------------------------
INSERT INTO TB_SYS_RIGHT (IDUSER, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT, USER_RIGHT)
SELECT u.IDUSER, f.FUNCTION_CODE, 0, 0, 0, 0, 0, 0
FROM TB_SYS_USER u
CROSS JOIN (
    SELECT 'F_LOGIN_DESKTOP' AS FUNCTION_CODE FROM DUAL
    UNION ALL SELECT 'F_LOGIN_WEB' FROM DUAL
    UNION ALL SELECT 'F_LOGIN_MOBILE' FROM DUAL
) f
WHERE NOT EXISTS (
    SELECT 1 FROM TB_SYS_RIGHT r
    WHERE r.IDUSER = u.IDUSER AND r.FUNCTION_CODE = f.FUNCTION_CODE
);

COMMIT;
PROMPT [MIGRATION V1_27] Default TB_SYS_RIGHT rows initialized.

-- ------------------------------------------------------------------------------
-- 4. Backfill quyền ban đầu từ CLIENT_TYPE (Chỉ chạy lần đầu nếu chưa cutover)
-- ------------------------------------------------------------------------------
DECLARE
    v_is_cutover NUMBER := 0;
BEGIN
    BEGIN
        SELECT COUNT(*) INTO v_is_cutover
        FROM TB_CONFIG
        WHERE NAME = 'PLATFORM_ACCESS_CUTOVER_APPLIED' AND UPPER(TRIM(VALUE)) IN ('1', 'TRUE');
    EXCEPTION
        WHEN OTHERS THEN v_is_cutover := 0;
    END;

    IF v_is_cutover = 0 THEN
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_27] First time cutover detected. Applying initial platform grants...');

        -- 4.1. Cấp quyền DESKTOP + WEB cho tài khoản ADMIN (Root Admin tuyệt đối không dùng Mobile)
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB')
          AND IDUSER IN (SELECT IDUSER FROM TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN');

        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 0, USER_RIGHT = 0, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE = 'F_LOGIN_MOBILE'
          AND IDUSER IN (SELECT IDUSER FROM TB_SYS_USER WHERE UPPER(TRIM(USERNAME)) = 'ADMIN');

        -- 4.2. Cấp quyền theo CLIENT_TYPE = 'DESKTOP'
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE = 'F_LOGIN_DESKTOP'
          AND IDUSER IN (
              SELECT IDUSER FROM TB_SYS_USER 
              WHERE UPPER(TRIM(CLIENT_TYPE)) = 'DESKTOP' 
                AND UPPER(TRIM(USERNAME)) <> 'ADMIN'
          );

        -- 4.3. Cấp quyền theo CLIENT_TYPE = 'WEB'
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE = 'F_LOGIN_WEB'
          AND IDUSER IN (
              SELECT IDUSER FROM TB_SYS_USER 
              WHERE UPPER(TRIM(CLIENT_TYPE)) = 'WEB' 
                AND UPPER(TRIM(USERNAME)) <> 'ADMIN'
          );

        -- 4.4. Cấp quyền theo CLIENT_TYPE = 'MOBILE' (Chỉ cấp khi có hồ sơ mapping)
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE = 'F_LOGIN_MOBILE'
          AND IDUSER IN (
              SELECT u.IDUSER FROM TB_SYS_USER u
              JOIN TB_USER_EMPLOYEE_MAPPING m ON u.IDUSER = m.USER_ID
              WHERE UPPER(TRIM(u.CLIENT_TYPE)) = 'MOBILE'
                AND UPPER(TRIM(u.USERNAME)) <> 'ADMIN'
                AND m.EMPLOYEE_ID IS NOT NULL
          );

        -- 4.5. Cấp quyền theo CLIENT_TYPE = 'SYSTEM' -> DESKTOP + WEB
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB')
          AND IDUSER IN (
              SELECT IDUSER FROM TB_SYS_USER 
              WHERE UPPER(TRIM(CLIENT_TYPE)) = 'SYSTEM' 
                AND UPPER(TRIM(USERNAME)) <> 'ADMIN'
          );

        -- 4.6. Cấp quyền theo CLIENT_TYPE = 'ALL' hoặc NULL:
        -- Nếu là tài khoản nhân viên (có mapping) -> cấp MOBILE + WEB
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE IN ('F_LOGIN_MOBILE', 'F_LOGIN_WEB')
          AND IDUSER IN (
              SELECT u.IDUSER FROM TB_SYS_USER u
              JOIN TB_USER_EMPLOYEE_MAPPING m ON u.IDUSER = m.USER_ID
              WHERE (u.CLIENT_TYPE IS NULL OR UPPER(TRIM(u.CLIENT_TYPE)) = 'ALL')
                AND UPPER(TRIM(u.USERNAME)) <> 'ADMIN'
                AND m.EMPLOYEE_ID IS NOT NULL
          );

        -- Nếu không có mapping (tài khoản văn phòng / quản lý) -> cấp DESKTOP + WEB
        UPDATE TB_SYS_RIGHT
        SET CAN_VIEW = 1, USER_RIGHT = 1, CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB')
          AND IDUSER IN (
              SELECT u.IDUSER FROM TB_SYS_USER u
              WHERE (u.CLIENT_TYPE IS NULL OR UPPER(TRIM(u.CLIENT_TYPE)) = 'ALL')
                AND UPPER(TRIM(u.USERNAME)) <> 'ADMIN'
                AND NOT EXISTS (SELECT 1 FROM TB_USER_EMPLOYEE_MAPPING m WHERE m.USER_ID = u.IDUSER AND m.EMPLOYEE_ID IS NOT NULL)
          );

        -- 4.7. Đảm bảo toàn bộ CAN_ADD/EDIT/DELETE/PRINT của F_LOGIN_* luôn bằng 0
        UPDATE TB_SYS_RIGHT
        SET CAN_ADD = 0, CAN_EDIT = 0, CAN_DELETE = 0, CAN_PRINT = 0
        WHERE FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE');

        -- Đánh dấu đã cutover thành công vào TB_CONFIG
        MERGE INTO TB_CONFIG tgt
        USING (SELECT 'PLATFORM_ACCESS_CUTOVER_APPLIED' AS NAME, 'TRUE' AS VALUE FROM DUAL) src
        ON (tgt.NAME = src.NAME)
        WHEN MATCHED THEN UPDATE SET tgt.VALUE = src.VALUE
        WHEN NOT MATCHED THEN INSERT (ID_CF, NAME, VALUE) 
            VALUES ((SELECT NVL(MAX(ID_CF), 0) + 1 FROM TB_CONFIG), src.NAME, src.VALUE);

        COMMIT;
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_27] Initial platform grants applied and cutover marker saved.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('[MIGRATION V1_27] Cutover was previously applied. Preserving existing admin rights.');
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 5. Bổ sung Trigger kiểm soát toàn vẹn cho 3 mã F_LOGIN_*
-- ------------------------------------------------------------------------------
CREATE OR REPLACE TRIGGER TRG_ENFORCE_LOGIN_RIGHTS
BEFORE INSERT OR UPDATE ON TB_SYS_RIGHT
FOR EACH ROW
BEGIN
    IF :NEW.FUNCTION_CODE IN ('F_LOGIN_DESKTOP', 'F_LOGIN_WEB', 'F_LOGIN_MOBILE') THEN
        -- Khóa tuyệt đối các action Add/Edit/Delete/Print
        :NEW.CAN_ADD := 0;
        :NEW.CAN_EDIT := 0;
        :NEW.CAN_DELETE := 0;
        :NEW.CAN_PRINT := 0;
        -- Đồng bộ USER_RIGHT với CAN_VIEW
        :NEW.USER_RIGHT := :NEW.CAN_VIEW;
    END IF;
END;
/

PROMPT [MIGRATION V1_27] Enforcement trigger created successfully.
COMMIT;
PROMPT [MIGRATION V1_27] MIGRATION COMPLETED SUCCESSFULLY.
EXIT;
