-- ==============================================================================
-- Migration V1_15: Enterprise Authentication, Session Management & Security Audit
-- Version: V1_15
-- Description:
--   1. Bổ sung các cột an ninh vào TB_SYS_USER:
--      - TOKEN_VERSION (Security version / stamp cấp tài khoản, default 1)
--      - FIRST_FAILED_LOGIN_AT, LAST_FAILED_LOGIN_AT, LAST_SUCCESS_LOGIN_AT
--      - LOCK_REASON
--   2. Tạo bảng quản lý phiên làm việc TB_AUTH_SESSION (JTI, Session State, Revocation)
--   3. Tạo bảng lưu vết đăng nhập TB_AUTH_LOGIN_ATTEMPT (Chống brute-force / audit)
--   4. Tạo bảng kiểm toán an ninh TB_AUTH_AUDIT (Audit Trail kèm Correlation ID)
--   5. Tạo bảng chính sách an ninh TB_AUTH_POLICY (Cấu hình hạn mức phiên & lockout)
--   6. Khởi tạo chính sách mặc định: GLOBAL (2 sessions), ROLE Admin (4 sessions)
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR CONTINUE;

-- ------------------------------------------------------------------------------
-- 1. Bổ sung các cột an ninh vào TB_SYS_USER
-- ------------------------------------------------------------------------------
DECLARE
    v_col_count NUMBER;
BEGIN
    -- 1.1 TOKEN_VERSION
    SELECT COUNT(*) INTO v_col_count FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'TOKEN_VERSION';
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (TOKEN_VERSION NUMBER(10) DEFAULT 1 NOT NULL)';
    END IF;

    -- 1.2 FIRST_FAILED_LOGIN_AT
    SELECT COUNT(*) INTO v_col_count FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'FIRST_FAILED_LOGIN_AT';
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (FIRST_FAILED_LOGIN_AT TIMESTAMP(6))';
    END IF;

    -- 1.3 LAST_FAILED_LOGIN_AT
    SELECT COUNT(*) INTO v_col_count FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'LAST_FAILED_LOGIN_AT';
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (LAST_FAILED_LOGIN_AT TIMESTAMP(6))';
    END IF;

    -- 1.4 LAST_SUCCESS_LOGIN_AT
    SELECT COUNT(*) INTO v_col_count FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'LAST_SUCCESS_LOGIN_AT';
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (LAST_SUCCESS_LOGIN_AT TIMESTAMP(6))';
    END IF;

    -- 1.5 LOCK_REASON
    SELECT COUNT(*) INTO v_col_count FROM user_tab_cols 
    WHERE table_name = 'TB_SYS_USER' AND column_name = 'LOCK_REASON';
    IF v_col_count = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_SYS_USER ADD (LOCK_REASON NVARCHAR2(255))';
    END IF;
END;
/

-- Backfill dữ liệu TOKEN_VERSION cho các user hiện có (đảm bảo không user nào bị null)
UPDATE TB_SYS_USER SET TOKEN_VERSION = 1 WHERE TOKEN_VERSION IS NULL OR TOKEN_VERSION = 0;
COMMIT;

-- ------------------------------------------------------------------------------
-- 2. Tạo bảng quản lý phiên làm việc TB_AUTH_SESSION
-- ------------------------------------------------------------------------------
DECLARE
    v_tbl_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_tbl_count FROM user_tables WHERE table_name = 'TB_AUTH_SESSION';
    IF v_tbl_count = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE TB_AUTH_SESSION (
            SESSION_ID NVARCHAR2(64) NOT NULL,
            USER_ID NUMBER NOT NULL,
            JTI NVARCHAR2(64) NOT NULL,
            CLIENT_TYPE NVARCHAR2(20) NOT NULL,
            PLATFORM NVARCHAR2(50),
            DEVICE_ID_HASH NVARCHAR2(64),
            DEVICE_NAME NVARCHAR2(100),
            IP_ADDRESS NVARCHAR2(50),
            USER_AGENT NVARCHAR2(500),
            CREATED_AT TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP NOT NULL,
            LAST_USED_AT TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP NOT NULL,
            EXPIRES_AT TIMESTAMP(6) NOT NULL,
            REVOKED_AT TIMESTAMP(6),
            REVOKE_REASON NVARCHAR2(100),
            CONSTRAINT PK_TB_AUTH_SESSION PRIMARY KEY (SESSION_ID),
            CONSTRAINT UQ_AUTH_SESSION_JTI UNIQUE (JTI),
            CONSTRAINT FK_AUTH_SESSION_USER FOREIGN KEY (USER_ID) REFERENCES TB_SYS_USER(IDUSER) ON DELETE CASCADE
        )';
    END IF;
END;
/

-- Tạo chỉ mục hiệu năng cho TB_AUTH_SESSION
DECLARE
    v_idx_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_SESSION_USER_STATE';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_SESSION_USER_STATE ON TB_AUTH_SESSION (USER_ID, REVOKED_AT, EXPIRES_AT)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;

    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_SESSION_USER_LASTUSED';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_SESSION_USER_LASTUSED ON TB_AUTH_SESSION (USER_ID, LAST_USED_AT)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 3. Tạo bảng lưu vết thử đăng nhập TB_AUTH_LOGIN_ATTEMPT
-- ------------------------------------------------------------------------------
DECLARE
    v_tbl_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_tbl_count FROM user_tables WHERE table_name = 'TB_AUTH_LOGIN_ATTEMPT';
    IF v_tbl_count = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE TB_AUTH_LOGIN_ATTEMPT (
            ATTEMPT_ID NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY,
            USER_ID NUMBER,
            LOGIN_IDENTIFIER NVARCHAR2(100) NOT NULL,
            CLIENT_TYPE NVARCHAR2(20),
            DEVICE_ID_HASH NVARCHAR2(64),
            IP_ADDRESS NVARCHAR2(50),
            USER_AGENT NVARCHAR2(500),
            ATTEMPTED_AT TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP NOT NULL,
            SUCCESS_FLAG NUMBER(1) NOT NULL,
            FAILURE_REASON NVARCHAR2(255),
            CORRELATION_ID NVARCHAR2(64),
            CONSTRAINT PK_TB_AUTH_LOGIN_ATTEMPT PRIMARY KEY (ATTEMPT_ID)
        )';
    END IF;
END;
/

-- Tạo chỉ mục hiệu năng cho TB_AUTH_LOGIN_ATTEMPT
DECLARE
    v_idx_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_ATTEMPT_USER_TIME';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_ATTEMPT_USER_TIME ON TB_AUTH_LOGIN_ATTEMPT (USER_ID, ATTEMPTED_AT)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;

    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_ATTEMPT_IDENT_TIME';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_ATTEMPT_IDENT_TIME ON TB_AUTH_LOGIN_ATTEMPT (LOGIN_IDENTIFIER, ATTEMPTED_AT)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;

    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_ATTEMPT_IP_TIME';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_ATTEMPT_IP_TIME ON TB_AUTH_LOGIN_ATTEMPT (IP_ADDRESS, ATTEMPTED_AT)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 4. Tạo bảng kiểm toán an ninh TB_AUTH_AUDIT
-- ------------------------------------------------------------------------------
DECLARE
    v_tbl_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_tbl_count FROM user_tables WHERE table_name = 'TB_AUTH_AUDIT';
    IF v_tbl_count = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE TB_AUTH_AUDIT (
            AUDIT_ID NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY,
            USER_ID NUMBER,
            ACTOR_USER_ID NUMBER,
            SESSION_ID NVARCHAR2(64),
            EVENT_TYPE NVARCHAR2(50) NOT NULL,
            RESULT NVARCHAR2(20) NOT NULL,
            REASON NVARCHAR2(255),
            CLIENT_TYPE NVARCHAR2(20),
            DEVICE_ID_HASH NVARCHAR2(64),
            IP_ADDRESS NVARCHAR2(50),
            USER_AGENT NVARCHAR2(500),
            OCCURRED_AT TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP NOT NULL,
            CORRELATION_ID NVARCHAR2(64),
            METADATA_JSON CLOB,
            CONSTRAINT PK_TB_AUTH_AUDIT PRIMARY KEY (AUDIT_ID)
        )';
    END IF;
END;
/

-- Tạo chỉ mục hiệu năng cho TB_AUTH_AUDIT
DECLARE
    v_idx_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_AUDIT_USER_TIME';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_AUDIT_USER_TIME ON TB_AUTH_AUDIT (USER_ID, OCCURRED_AT)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;

    SELECT COUNT(*) INTO v_idx_count FROM user_indexes WHERE index_name = 'IX_AUTH_AUDIT_CORRELATION';
    IF v_idx_count = 0 THEN
        BEGIN
            EXECUTE IMMEDIATE 'CREATE INDEX IX_AUTH_AUDIT_CORRELATION ON TB_AUTH_AUDIT (CORRELATION_ID)';
        EXCEPTION WHEN OTHERS THEN NULL;
        END;
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 5. Tạo bảng cấu hình chính sách an ninh TB_AUTH_POLICY
-- ------------------------------------------------------------------------------
DECLARE
    v_tbl_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_tbl_count FROM user_tables WHERE table_name = 'TB_AUTH_POLICY';
    IF v_tbl_count = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE TB_AUTH_POLICY (
            POLICY_ID NUMBER GENERATED BY DEFAULT ON NULL AS IDENTITY,
            SCOPE_TYPE NVARCHAR2(20) NOT NULL,
            SCOPE_ID NVARCHAR2(50) NOT NULL,
            MAX_ACTIVE_SESSIONS NUMBER(3) DEFAULT 2 NOT NULL,
            SESSION_LIMIT_STRATEGY NVARCHAR2(20) DEFAULT ''REVOKE_OLDEST'' NOT NULL,
            ACCESS_TOKEN_MINUTES NUMBER(6) DEFAULT 60 NOT NULL,
            IDLE_TIMEOUT_MINUTES NUMBER(6) DEFAULT 480 NOT NULL,
            ABSOLUTE_TIMEOUT_MINUTES NUMBER(6) DEFAULT 1440 NOT NULL,
            MAX_FAILED_LOGIN_ATTEMPTS NUMBER(3) DEFAULT 5 NOT NULL,
            FAILED_ATTEMPT_WINDOW_MINUTES NUMBER(4) DEFAULT 15 NOT NULL,
            LOCKOUT_DURATION_MINUTES NUMBER(4) DEFAULT 15 NOT NULL,
            ENABLED NUMBER(1) DEFAULT 1 NOT NULL,
            CREATED_AT TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP NOT NULL,
            UPDATED_AT TIMESTAMP(6) DEFAULT CURRENT_TIMESTAMP NOT NULL,
            CONSTRAINT PK_TB_AUTH_POLICY PRIMARY KEY (POLICY_ID),
            CONSTRAINT UQ_AUTH_POLICY_SCOPE UNIQUE (SCOPE_TYPE, SCOPE_ID)
        )';
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 6. Khởi tạo chính sách mặc định (Seed Defaults)
-- ------------------------------------------------------------------------------
MERGE INTO TB_AUTH_POLICY target
USING (
    SELECT 'GLOBAL' AS SCOPE_TYPE, '*' AS SCOPE_ID, 2 AS MAX_ACTIVE_SESSIONS, 'REVOKE_OLDEST' AS SESSION_LIMIT_STRATEGY, 60 AS ACCESS_TOKEN_MINUTES, 480 AS IDLE_TIMEOUT_MINUTES, 1440 AS ABSOLUTE_TIMEOUT_MINUTES, 5 AS MAX_FAILED_LOGIN_ATTEMPTS, 15 AS FAILED_ATTEMPT_WINDOW_MINUTES, 15 AS LOCKOUT_DURATION_MINUTES FROM DUAL
    UNION ALL
    SELECT 'ROLE' AS SCOPE_TYPE, 'Admin' AS SCOPE_ID, 4 AS MAX_ACTIVE_SESSIONS, 'REVOKE_OLDEST' AS SESSION_LIMIT_STRATEGY, 120 AS ACCESS_TOKEN_MINUTES, 480 AS IDLE_TIMEOUT_MINUTES, 1440 AS ABSOLUTE_TIMEOUT_MINUTES, 5 AS MAX_FAILED_LOGIN_ATTEMPTS, 15 AS FAILED_ATTEMPT_WINDOW_MINUTES, 15 AS LOCKOUT_DURATION_MINUTES FROM DUAL
) source
ON (target.SCOPE_TYPE = source.SCOPE_TYPE AND target.SCOPE_ID = source.SCOPE_ID)
WHEN NOT MATCHED THEN
    INSERT (SCOPE_TYPE, SCOPE_ID, MAX_ACTIVE_SESSIONS, SESSION_LIMIT_STRATEGY, ACCESS_TOKEN_MINUTES, IDLE_TIMEOUT_MINUTES, ABSOLUTE_TIMEOUT_MINUTES, MAX_FAILED_LOGIN_ATTEMPTS, FAILED_ATTEMPT_WINDOW_MINUTES, LOCKOUT_DURATION_MINUTES, ENABLED, CREATED_AT, UPDATED_AT)
    VALUES (source.SCOPE_TYPE, source.SCOPE_ID, source.MAX_ACTIVE_SESSIONS, source.SESSION_LIMIT_STRATEGY, source.ACCESS_TOKEN_MINUTES, source.IDLE_TIMEOUT_MINUTES, source.ABSOLUTE_TIMEOUT_MINUTES, source.MAX_FAILED_LOGIN_ATTEMPTS, source.FAILED_ATTEMPT_WINDOW_MINUTES, source.LOCKOUT_DURATION_MINUTES, 1, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

COMMIT;
EXIT;

