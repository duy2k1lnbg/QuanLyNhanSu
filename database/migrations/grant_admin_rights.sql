-- ============================================================================
-- [DEPRECATED - CANH BAO KHONG SU DUNG TRUC TIEP]
-- Script nay chua cac ID co dinh 80 va 89 chi ap dung cho moi truong dev cu the.
-- KHONG chay script nay tren cac moi truong khac vi IDUSER cua ADMIN co the khac.
-- Vui long su dung migration dong: V1_30__admin_channel_print_grants_and_preflight.sql
-- ============================================================================

UPDATE HR.TB_SYS_RIGHT 
   SET USER_RIGHT = 1, CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1 
 WHERE IDUSER IN (80, 89);

-- Dam bao co F_SYSTEM_AI va F_SYSTEM_AI_CONFIG cho IDUSER = 80
MERGE INTO HR.TB_SYS_RIGHT t
USING (
    SELECT 80 AS uid, 'F_SYSTEM_AI' AS fcode FROM DUAL UNION ALL
    SELECT 80, 'F_SYSTEM_AI_CONFIG' FROM DUAL UNION ALL
    SELECT 89, 'F_SYSTEM_AI' FROM DUAL UNION ALL
    SELECT 89, 'F_SYSTEM_AI_CONFIG' FROM DUAL
) s ON (t.IDUSER = s.uid AND t.FUNCTION_CODE = s.fcode)
WHEN MATCHED THEN UPDATE SET t.USER_RIGHT = 1, t.CAN_VIEW = 1, t.CAN_ADD = 1, t.CAN_EDIT = 1, t.CAN_DELETE = 1
WHEN NOT MATCHED THEN INSERT (IDUSER, FUNCTION_CODE, USER_RIGHT, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
VALUES (s.uid, s.fcode, 1, 1, 1, 1, 1, 1);

COMMIT;
EXIT;
