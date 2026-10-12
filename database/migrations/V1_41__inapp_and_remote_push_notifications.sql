-- ==============================================================================
-- Migration V1_41: In-App & Remote Push Notifications, Multi-Device Registry & Outbox
-- File: database/migrations/V1_41__inapp_and_remote_push_notifications.sql
-- Description:
--   1. Bổ sung các cột phân biệt người nhận (TARGET_TYPE, TARGET_MANV, EVENT_TYPE, ENTITY_ID)
--      vào TB_THONGBAO; tạo SEQ_THONGBAO an toàn cho concurrent transactions.
--   2. Tạo bảng TB_USER_DEVICE và SEQ_USER_DEVICE: Quản lý đa thiết bị, installationId,
--      Expo Push Token, trạng thái quyền OS, toggle Push trong HRMS.
--   3. Tạo bảng TB_PUSH_OUTBOX và SEQ_PUSH_OUTBOX: Transactional Outbox đảm bảo tin cậy,
--      chống gửi trùng (idempotency key), hỗ trợ lock, retry backoff, ticket & receipt.
--   4. Ghi nhận phiên bản SCHEMA_VERSION 1.41.
-- ==============================================================================

SET DEFINE OFF;
WHENEVER SQLERROR EXIT FAILURE ROLLBACK;

PROMPT [MIGRATION V1_41] Starting In-App & Remote Push Notifications Migration...

-- ------------------------------------------------------------------------------
-- 1. NÂNG CẤP TB_THONGBAO & TẠO SEQUENCE SEQ_THONGBAO
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
    v_max_id NUMBER := 0;
BEGIN
    -- Tạo SEQUENCE SEQ_THONGBAO an toàn nếu chưa có
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_THONGBAO';
    IF v_cnt = 0 THEN
        SELECT NVL(MAX(ID), 0) INTO v_max_id FROM TB_THONGBAO;
        v_max_id := v_max_id + 1;
        EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_THONGBAO START WITH ' || v_max_id || ' INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_THONGBAO starting at ' || v_max_id);
    END IF;

    -- Thêm cột TARGET_TYPE
    SELECT COUNT(*) INTO v_cnt FROM user_tab_cols WHERE table_name = 'TB_THONGBAO' AND column_name = 'TARGET_TYPE';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_THONGBAO ADD (TARGET_TYPE VARCHAR2(30) DEFAULT ''ALL'' NOT NULL)';
        DBMS_OUTPUT.PUT_LINE('Added column TARGET_TYPE to TB_THONGBAO');
    END IF;

    -- Thêm cột TARGET_MANV
    SELECT COUNT(*) INTO v_cnt FROM user_tab_cols WHERE table_name = 'TB_THONGBAO' AND column_name = 'TARGET_MANV';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_THONGBAO ADD (TARGET_MANV NUMBER NULL)';
        DBMS_OUTPUT.PUT_LINE('Added column TARGET_MANV to TB_THONGBAO');
    END IF;

    -- Thêm cột EVENT_TYPE
    SELECT COUNT(*) INTO v_cnt FROM user_tab_cols WHERE table_name = 'TB_THONGBAO' AND column_name = 'EVENT_TYPE';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_THONGBAO ADD (EVENT_TYPE VARCHAR2(50) NULL)';
        DBMS_OUTPUT.PUT_LINE('Added column EVENT_TYPE to TB_THONGBAO');
    END IF;

    -- Thêm cột ENTITY_ID
    SELECT COUNT(*) INTO v_cnt FROM user_tab_cols WHERE table_name = 'TB_THONGBAO' AND column_name = 'ENTITY_ID';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_THONGBAO ADD (ENTITY_ID NUMBER NULL)';
        DBMS_OUTPUT.PUT_LINE('Added column ENTITY_ID to TB_THONGBAO');
    END IF;

    -- Thêm check constraint cho TARGET_TYPE nếu chưa có
    SELECT COUNT(*) INTO v_cnt FROM user_constraints WHERE constraint_name = 'CK_TB_TARGET_TYPE';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'ALTER TABLE TB_THONGBAO ADD CONSTRAINT CK_TB_TARGET_TYPE CHECK (TARGET_TYPE IN (''ALL'', ''COMPANY'', ''DEPARTMENT'', ''PERSONAL''))';
    END IF;

    -- Tạo index trên TARGET_MANV để tối ưu hóa truy vấn cá nhân
    SELECT COUNT(*) INTO v_cnt FROM user_indexes WHERE index_name = 'IX_THONGBAO_TARGET';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE INDEX IX_THONGBAO_TARGET ON TB_THONGBAO (TARGET_TYPE, TARGET_MANV, MAPB, MACTY)';
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 2. TẠO BẢNG TB_USER_DEVICE VÀ SEQUENCE SEQ_USER_DEVICE
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_USER_DEVICE';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_USER_DEVICE START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_USER_DEVICE');
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'TB_USER_DEVICE';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE TB_USER_DEVICE (
            ID                  NUMBER NOT NULL,
            IDUSER              NUMBER NOT NULL,
            MANV                NUMBER NULL,
            INSTALLATION_ID     VARCHAR2(100) NOT NULL,
            EXPO_PUSH_TOKEN     VARCHAR2(255) NULL,
            PLATFORM            VARCHAR2(30) NOT NULL,
            PROJECT_ID          VARCHAR2(100) NULL,
            ENVIRONMENT         VARCHAR2(30) DEFAULT ''PRODUCTION'' NOT NULL,
            LANGUAGE            VARCHAR2(10) DEFAULT ''vi'' NOT NULL,
            APP_VERSION         VARCHAR2(50) NULL,
            PERMISSION_STATUS   VARCHAR2(30) DEFAULT ''UNDETERMINED'' NOT NULL,
            PUSH_ENABLED        NUMBER(1) DEFAULT 1 NOT NULL,
            IS_ACTIVE           NUMBER(1) DEFAULT 1 NOT NULL,
            DEVICE_NAME         VARCHAR2(150) NULL,
            CREATED_AT          DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT          DATE DEFAULT SYSDATE NOT NULL,
            LAST_SEEN_AT        DATE DEFAULT SYSDATE NOT NULL,
            CONSTRAINT PK_USER_DEVICE PRIMARY KEY (ID),
            CONSTRAINT UQ_USER_DEVICE_INSTALL UNIQUE (INSTALLATION_ID),
            CONSTRAINT CK_USER_DEV_PUSH_EN CHECK (PUSH_ENABLED IN (0, 1)),
            CONSTRAINT CK_USER_DEV_ACTIVE CHECK (IS_ACTIVE IN (0, 1)),
            CONSTRAINT CK_USER_DEV_PERM CHECK (PERMISSION_STATUS IN (''GRANTED'', ''DENIED'', ''UNDETERMINED''))
        )';
        DBMS_OUTPUT.PUT_LINE('Created TABLE TB_USER_DEVICE');

        EXECUTE IMMEDIATE 'CREATE INDEX IX_USER_DEV_LOOKUP ON TB_USER_DEVICE (IDUSER, IS_ACTIVE, PUSH_ENABLED)';
        EXECUTE IMMEDIATE 'CREATE INDEX IX_USER_DEV_MANV ON TB_USER_DEVICE (MANV, IS_ACTIVE, PUSH_ENABLED)';
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 3. TẠO BẢNG TB_PUSH_OUTBOX VÀ SEQUENCE SEQ_PUSH_OUTBOX
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_sequences WHERE sequence_name = 'SEQ_PUSH_OUTBOX';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE 'CREATE SEQUENCE SEQ_PUSH_OUTBOX START WITH 1 INCREMENT BY 1 NOCACHE NOCYCLE';
        DBMS_OUTPUT.PUT_LINE('Created SEQUENCE SEQ_PUSH_OUTBOX');
    END IF;

    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'TB_PUSH_OUTBOX';
    IF v_cnt = 0 THEN
        EXECUTE IMMEDIATE '
        CREATE TABLE TB_PUSH_OUTBOX (
            ID                  NUMBER NOT NULL,
            NOTIFICATION_ID     NUMBER NOT NULL,
            DEVICE_ID           NUMBER NOT NULL,
            IDUSER              NUMBER NOT NULL,
            INSTALLATION_ID     VARCHAR2(100) NOT NULL,
            EXPO_PUSH_TOKEN     VARCHAR2(255) NOT NULL,
            TITLE               NVARCHAR2(255) NOT NULL,
            BODY                NVARCHAR2(1000) NOT NULL,
            DATA_JSON           NCLOB NULL,
            STATUS              VARCHAR2(30) DEFAULT ''PENDING'' NOT NULL,
            RETRY_COUNT         NUMBER DEFAULT 0 NOT NULL,
            MAX_RETRIES         NUMBER DEFAULT 5 NOT NULL,
            NEXT_RETRY_AT       DATE DEFAULT SYSDATE NOT NULL,
            LOCKED_UNTIL        DATE NULL,
            LOCKED_BY           VARCHAR2(100) NULL,
            TICKET_ID           VARCHAR2(100) NULL,
            TICKET_STATUS       VARCHAR2(30) NULL,
            TICKET_ERROR        VARCHAR2(100) NULL,
            RECEIPT_STATUS      VARCHAR2(30) NULL,
            RECEIPT_ERROR       VARCHAR2(100) NULL,
            RECEIPT_CHECKED_AT  DATE NULL,
            LAST_ERROR          NVARCHAR2(1000) NULL,
            IDEMPOTENCY_KEY     VARCHAR2(200) NOT NULL,
            CREATED_AT          DATE DEFAULT SYSDATE NOT NULL,
            SENT_AT             DATE NULL,
            CONSTRAINT PK_PUSH_OUTBOX PRIMARY KEY (ID),
            CONSTRAINT UQ_PUSH_OUTBOX_KEY UNIQUE (IDEMPOTENCY_KEY),
            CONSTRAINT CK_PUSH_OUTBOX_STATUS CHECK (STATUS IN (''PENDING'', ''PROCESSING'', ''TICKET_OK'', ''RECEIPT_OK'', ''ERROR'', ''FAILED_FATAL'', ''SKIPPED''))
        )';
        DBMS_OUTPUT.PUT_LINE('Created TABLE TB_PUSH_OUTBOX');

        EXECUTE IMMEDIATE 'CREATE INDEX IX_PUSH_OUTBOX_DISPATCH ON TB_PUSH_OUTBOX (STATUS, NEXT_RETRY_AT, LOCKED_UNTIL)';
        EXECUTE IMMEDIATE 'CREATE INDEX IX_PUSH_OUTBOX_TICKET ON TB_PUSH_OUTBOX (TICKET_ID)';
        EXECUTE IMMEDIATE 'CREATE INDEX IX_PUSH_OUTBOX_NOTIF ON TB_PUSH_OUTBOX (NOTIFICATION_ID)';
    END IF;
END;
/

-- ------------------------------------------------------------------------------
-- 4. GHI NHẬN PHIÊN BẢN VÀO SCHEMA_VERSION
-- ------------------------------------------------------------------------------
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM user_tables WHERE table_name = 'SCHEMA_VERSION';
    IF v_cnt > 0 THEN
        SELECT COUNT(*) INTO v_cnt FROM SCHEMA_VERSION WHERE VERSION = '1.41';
        IF v_cnt = 0 THEN
            INSERT INTO SCHEMA_VERSION (VERSION, DESCRIPTION, SCRIPT, SUCCESS)
            VALUES ('1.41', 'In-App & Remote Push Notifications with Device Registry and Transactional Outbox', 'V1_41__inapp_and_remote_push_notifications.sql', 1);
        END IF;
    END IF;
END;
/

COMMIT;
PROMPT [MIGRATION V1_41] In-App & Remote Push Notifications Migration Completed Successfully!
