-- ==============================================================================
-- Migration V1_46: Standardize Approval & Disbursement Functions, Hierarchy & Channel Grants
-- File: database/migrations/V1_46__standardize_approval_and_disbursement_functions.sql
-- Description:
--   1. Chuẩn hóa metadata (DESCRIPTION, PARENT, SORT_ORDER, ISGROUP) trong HR.TB_SYS_FUNCTION
--      cho các chức năng duyệt và giải ngân:
--      - F_NV_PHEDUYET: Phê duyệt yêu cầu trực tuyến (Parent: NV)
--      - F_APPROVE_ADVANCE: Phê duyệt đơn tạm ứng lương (Parent: CC)
--      - F_APPROVE_PROFILE: Phê duyệt thay đổi hồ sơ nhân viên (Parent: NV)
--      - F_PAYROLL_DISBURSE_ADVANCE: Xác nhận chi tiền tạm ứng lương (Parent: CC)
--      - Khắc phục triệt để lỗi mã hóa (mojibake) tiếng Việt.
--   2. Đảm bảo cấu hình channel capabilities và baseline grants trong TB_SYS_RIGHT và TB_SYS_RIGHT_CHANNEL
--      cho quản trị viên theo chuẩn Zero-Trust / Fail-Closed.
--   3. Ghi nhận phiên bản SCHEMA_VERSION 1.46.
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

PROMPT [MIGRATION V1_46] Standardizing Approval & Disbursement Functions...

-- 1. Chuẩn hóa metadata trong HR.TB_SYS_FUNCTION
MERGE INTO HR.TB_SYS_FUNCTION tgt
USING (
    SELECT 'F_NV_PHEDUYET' AS c, TO_NCHAR('Phê Duyệt Yêu Cầu (Online)') AS d, TO_NCHAR('NV') AS p, 'FUNCTION' AS t, 0 AS g, 181 AS s FROM DUAL UNION ALL
    SELECT 'F_APPROVE_ADVANCE', TO_NCHAR('Phê duyệt đơn tạm ứng lương'), TO_NCHAR('CC'), 'FUNCTION', 0, 221 FROM DUAL UNION ALL
    SELECT 'F_APPROVE_PROFILE', TO_NCHAR('Phê duyệt thay đổi hồ sơ nhân viên'), TO_NCHAR('NV'), 'FUNCTION', 0, 223 FROM DUAL UNION ALL
    SELECT 'F_PAYROLL_DISBURSE_ADVANCE', TO_NCHAR('Xác nhận chi tiền tạm ứng lương'), TO_NCHAR('CC'), 'FUNCTION', 0, 222 FROM DUAL
) src
ON (tgt.FUNCTION_CODE = src.c)
WHEN MATCHED THEN
    UPDATE SET 
        tgt.DESCRIPTION = src.d,
        tgt.PARENT = src.p,
        tgt.RIGHT_TYPE = src.t,
        tgt.ISGROUP = src.g,
        tgt.SORT = src.s
WHEN NOT MATCHED THEN
    INSERT (FUNCTION_CODE, DESCRIPTION, PARENT, RIGHT_TYPE, ISGROUP, SORT)
    VALUES (src.c, src.d, src.p, src.t, src.g, src.s);

COMMIT;
PROMPT [MIGRATION V1_46] Updated TB_SYS_FUNCTION metadata.

-- 2. Đồng bộ quyền mặc định (TB_SYS_RIGHT & TB_SYS_RIGHT_CHANNEL) cho quản trị viên (Admin)
DECLARE
    v_admin_id NUMBER := 1;
BEGIN
    SELECT MIN(IDUSER) INTO v_admin_id FROM HR.TB_SYS_USER WHERE LOWER(USERNAME) = 'admin';
    IF v_admin_id IS NULL THEN
        v_admin_id := 1;
    END IF;

    -- A. Cấp quyền trực tiếp trên TB_SYS_RIGHT
    FOR r IN (
        SELECT FUNCTION_CODE FROM HR.TB_SYS_FUNCTION 
        WHERE FUNCTION_CODE IN ('F_NV_PHEDUYET', 'F_APPROVE_ADVANCE', 'F_APPROVE_PROFILE', 'F_PAYROLL_DISBURSE_ADVANCE')
    ) LOOP
        MERGE INTO HR.TB_SYS_RIGHT tgt
        USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID FROM DUAL) src
        ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID)
        WHEN MATCHED THEN
            UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1, USER_RIGHT = 1
        WHEN NOT MATCHED THEN
            INSERT (FUNCTION_CODE, IDUSER, USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.FC, src.U_ID, 1, 1, 1, 1, 1, 1);

        -- B. Kênh DESKTOP
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID, 'DESKTOP' AS CT FROM DUAL) src
        ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
        WHEN MATCHED THEN
            UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.U_ID, src.CT, src.FC, 1, 1, 1, 1, 1);

        -- C. Kênh WEB
        MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
        USING (SELECT r.FUNCTION_CODE AS FC, v_admin_id AS U_ID, 'WEB' AS CT FROM DUAL) src
        ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
        WHEN MATCHED THEN
            UPDATE SET CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1
        WHEN NOT MATCHED THEN
            INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (src.U_ID, src.CT, src.FC, 1, 1, 1, 1, 1);
    END LOOP;

    -- D. Kênh WEB: đảm bảo F_SYSTEM_GIAMSAT cho Admin
    MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
    USING (SELECT 'F_SYSTEM_GIAMSAT' AS FC, v_admin_id AS U_ID, 'WEB' AS CT FROM DUAL) src
    ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
    WHEN MATCHED THEN
        UPDATE SET CAN_VIEW = 1, CAN_EDIT = 1
    WHEN NOT MATCHED THEN
        INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
        VALUES (src.U_ID, src.CT, src.FC, 1, 0, 1, 0, 0);

    -- E. Kênh WEB: đảm bảo quyền in ấn Bảng lương (F_CC_BANGLUONG) cho Admin
    MERGE INTO HR.TB_SYS_RIGHT_CHANNEL tgt
    USING (SELECT 'F_CC_BANGLUONG' AS FC, v_admin_id AS U_ID, 'WEB' AS CT FROM DUAL) src
    ON (tgt.FUNCTION_CODE = src.FC AND tgt.IDUSER = src.U_ID AND tgt.CLIENT_TYPE = src.CT)
    WHEN MATCHED THEN
        UPDATE SET CAN_VIEW = 1, CAN_PRINT = 1
    WHEN NOT MATCHED THEN
        INSERT (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
        VALUES (src.U_ID, src.CT, src.FC, 1, 0, 0, 0, 1);

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Granted standard channel permissions to Admin.');
END;
/

-- 3. Ghi nhận SCHEMA_VERSION 1.46
INSERT INTO SCHEMA_VERSION (VERSION, DESCRIPTION, TYPE, SCRIPT, INSTALLED_BY, INSTALLED_ON, SUCCESS)
SELECT '1.46', 'Standardize approval and disbursement function metadata, parent hierarchy, and channel grants', 'SQL', 'V1_46__standardize_approval_and_disbursement_functions.sql', USER, SYSTIMESTAMP, 1
FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM SCHEMA_VERSION WHERE VERSION = '1.46' AND SUCCESS = 1);

COMMIT;
PROMPT [MIGRATION V1_46] Completed successfully.
EXIT;
