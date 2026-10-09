-- ==============================================================================
-- Verify V1_28: Channel Rights & Function Types Verification Script
-- File: database/migrations/V1_28_verify__channel_rights_and_function_types.sql
-- Description:
--   Kiểm tra tính đúng đắn và toàn vẹn của cấu trúc CSDL sau migration V1_28
--   (Chỉ đọc - READ ONLY - không ghi bất kỳ dữ liệu nào).
-- ==============================================================================

SET PAGESIZE 50;
SET LINESIZE 200;

PROMPT ==============================================================================
PROMPT 1. KIEM TRA COT RIGHT_TYPE TRONG TB_SYS_FUNCTION
PROMPT ==============================================================================
SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE, DATA_DEFAULT
FROM ALL_TAB_COLUMNS
WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_FUNCTION' AND COLUMN_NAME = 'RIGHT_TYPE';

PROMPT ==============================================================================
PROMPT 2. KIEM TRA PHAN BO RIGHT_TYPE THEO NHOM CHUC NANG
PROMPT ==============================================================================
SELECT RIGHT_TYPE, COUNT(*) AS TOTAL_FUNCTIONS
FROM HR.TB_SYS_FUNCTION
GROUP BY RIGHT_TYPE
ORDER BY RIGHT_TYPE;

PROMPT ==============================================================================
PROMPT 3. KIEM TRA BANG TB_SYS_RIGHT_CHANNEL VA SO DONG THEO KENH
PROMPT ==============================================================================
SELECT CLIENT_TYPE, COUNT(*) AS TOTAL_GRANTS,
       SUM(CAN_VIEW) AS SUM_VIEW,
       SUM(CAN_ADD) AS SUM_ADD,
       SUM(CAN_EDIT) AS SUM_EDIT,
       SUM(CAN_DELETE) AS SUM_DELETE,
       SUM(CAN_PRINT) AS SUM_PRINT
FROM HR.TB_SYS_RIGHT_CHANNEL
GROUP BY CLIENT_TYPE
ORDER BY CLIENT_TYPE;

PROMPT ==============================================================================
PROMPT 4. KIEM TRA TOAN VEN: WEB KHONG DUOC CO QUYEN GHI BANG LUONG (F_CC_BANGLUONG)
PROMPT ==============================================================================
SELECT COUNT(*) AS VIOLATION_COUNT
FROM HR.TB_SYS_RIGHT_CHANNEL
WHERE CLIENT_TYPE = 'WEB'
  AND FUNCTION_CODE = 'F_CC_BANGLUONG'
  AND (CAN_ADD > 0 OR CAN_EDIT > 0 OR CAN_DELETE > 0 OR CAN_PRINT > 0);

PROMPT ==============================================================================
PROMPT 5. KIEM TRA CO CUTOVER V1_28 TRONG TB_CONFIG
PROMPT ==============================================================================
SELECT NAME, VALUE FROM HR.TB_CONFIG WHERE NAME = 'CHANNEL_RIGHTS_V1_28_APPLIED';

PROMPT ==============================================================================
PROMPT 6. KIEM TRA TRIGGER TRG_ENFORCE_RIGHT_CHANNEL
PROMPT ==============================================================================
SELECT TRIGGER_NAME, STATUS, TRIGGER_TYPE, TRIGGERING_EVENT
FROM ALL_TRIGGERS
WHERE OWNER = 'HR' AND TRIGGER_NAME = 'TRG_ENFORCE_RIGHT_CHANNEL';
