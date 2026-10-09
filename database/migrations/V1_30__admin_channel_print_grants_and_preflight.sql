-- ============================================================================
-- SCRIPT: V1_30__admin_channel_print_grants_and_preflight.sql
-- Muc dich: Bo sung 46 quyen In (DESKTOP) va 34 quyen In (WEB) con thieu cho ADMIN
--           theo dung ChannelCapabilityRegistry va Channel RBAC Zero-Trust.
-- Nguoi chay: DBA / Quan tri he thong co quyen tren schema HR
-- Luu y: KHONG tu chay tren Production; chi chay khi co cua so bao tri va xac nhan.
--        Tim kiem dong qua UPPER(TRIM(USERNAME)) = 'ADMIN', KHONG hardcode ID=80/89.
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT [1/5] Preflight Check: Tim kiem tai khoan ADMIN dong...
DECLARE
    v_admin_count NUMBER;
    v_admin_id    NUMBER;
    v_admin_name  VARCHAR2(100);
    v_tbl_count   NUMBER;
BEGIN
    -- 1. Kiem tra bang TB_SYS_RIGHT_CHANNEL
    SELECT COUNT(*) INTO v_tbl_count
    FROM ALL_TABLES
    WHERE OWNER = 'HR' AND TABLE_NAME = 'TB_SYS_RIGHT_CHANNEL';

    IF v_tbl_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Preflight that bai: Bang HR.TB_SYS_RIGHT_CHANNEL khong ton tai. Hay chay V1_28 truoc.');
    END IF;

    -- 2. Tim duy nhat mot tai khoan ADMIN
    SELECT COUNT(*) INTO v_admin_count
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    IF v_admin_count = 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Preflight that bai: Khong tim thay tai khoan ADMIN trong TB_SYS_USER.');
    ELSIF v_admin_count > 1 THEN
        RAISE_APPLICATION_ERROR(-20003, 'Preflight that bai: Phat hien nhieu hon 1 tai khoan co USERNAME la ADMIN.');
    END IF;

    SELECT IDUSER, USERNAME INTO v_admin_id, v_admin_name
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    DBMS_OUTPUT.PUT_LINE('-> Preflight thanh cong: Tim thay tai khoan ADMIN ID=' || v_admin_id || ' (' || v_admin_name || ')');
END;
/

PROMPT [2/5] Cap 46 quyen In (CAN_PRINT = 1) tren kenh DESKTOP cho ADMIN...
DECLARE
    v_admin_id NUMBER;
    v_updated  NUMBER := 0;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    -- Danh sach 46 chuc nang ho tro In tren Desktop theo ChannelCapabilityRegistry
    FOR r IN (
        SELECT COLUMN_VALUE AS FCODE FROM TABLE(SYS.ODCIVARCHAR2LIST(
            'F_DM_BOPHAN', 'F_DM_CHUCVU', 'F_DM_CONGTY', 'F_DM_DANTOC', 'F_DM_NHANVIEN',
            'F_DM_PHONGBAN', 'F_DM_TONGIAO', 'F_DM_TRINHDO', 'F_DM_LOAIPHEP',
            'F_NV_LOAIHOPDONG', 'F_NV_DIEUCHUYEN', 'F_NV_HOPDONG', 'F_NV_KHENTHUONG',
            'F_NV_KYLUAT', 'F_NV_NANGLUONG', 'F_NV_THOIVIEC', 'F_NV_NGHIPHEP',
            'F_CC_BANGCONG', 'F_CC_BANGLUONG', 'F_CC_BCCT', 'F_CC_LOAICA', 'F_CC_LOAICONG',
            'F_CC_PHUCAP', 'F_CC_TANGCA', 'F_CC_UNGLUONG', 'F_CC_NGAYLE', 'F_CC_BCCT_IN', 'F_CC_CAPNHATCONG',
            'F_BC_BAOCAO', 'F_DB_LUONG', 'F_DB_NHANSU',
            'F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'F_SYSTEM_PHUCHOI', 'F_SYSTEM_SAULUU',
            'F_SYSTEM_LOCK_USER', 'F_SYSTEM_SETTING', 'F_SYSTEM_GIAMSAT', 'F_SYSTEM_AI',
            'F_SYSTEM_AI_CONFIG', 'F_SYSTEM_CAPTAIKHOAN', 'F_SYSTEM_PQ_CHUCNANG',
            'F_SYSTEM_PQ_BAOCAO', 'F_SYSTEM_THONGBAO', 'F_SYSTEM_DB_CONFIG', 'DOIMATKHAU'
        ))
    ) LOOP
        UPDATE HR.TB_SYS_RIGHT_CHANNEL
        SET CAN_PRINT = 1
        WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'DESKTOP' AND FUNCTION_CODE = r.FCODE;

        IF SQL%ROWCOUNT = 0 THEN
            INSERT INTO HR.TB_SYS_RIGHT_CHANNEL (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (v_admin_id, 'DESKTOP', r.FCODE, 1, 1, 1, 1, 1);
        END IF;
        v_updated := v_updated + 1;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('-> Da cap nhat / dong bo CAN_PRINT=1 cho ' || v_updated || ' chuc nang tren kenh DESKTOP.');
END;
/

PROMPT [3/5] Cap 34 quyen In (CAN_PRINT = 1) tren kenh WEB cho ADMIN...
DECLARE
    v_admin_id NUMBER;
    v_updated  NUMBER := 0;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    -- Danh sach 34 chuc nang ho tro In tren Web theo ChannelCapabilityRegistry
    -- Luu y: F_CC_BANGLUONG tren Web chi co quyen View (CAN_PRINT = 0), tuyet doi khong cap!
    FOR r IN (
        SELECT COLUMN_VALUE AS FCODE FROM TABLE(SYS.ODCIVARCHAR2LIST(
            'F_DM_BOPHAN', 'F_DM_CHUCVU', 'F_DM_CONGTY', 'F_DM_DANTOC', 'F_DM_NHANVIEN',
            'F_DM_PHONGBAN', 'F_DM_TONGIAO', 'F_DM_TRINHDO', 'F_DM_LOAIPHEP',
            'F_NV_LOAIHOPDONG', 'F_NV_DIEUCHUYEN', 'F_NV_HOPDONG', 'F_NV_KHENTHUONG',
            'F_NV_KYLUAT', 'F_NV_NANGLUONG', 'F_NV_THOIVIEC', 'F_NV_NGHIPHEP',
            'F_CC_BANGCONG', 'F_CC_BCCT', 'F_CC_NGAYLE',
            'F_BC_BAOCAO', 'F_DB_LUONG', 'F_DB_NHANSU',
            'F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'F_SYSTEM_LOCK_USER', 'F_SYSTEM_CAPTAIKHOAN',
            'F_SYSTEM_PQ_CHUCNANG', 'F_SYSTEM_PQ_BAOCAO', 'F_SYSTEM_THONGBAO',
            'F_SYSTEM_AI', 'F_SYSTEM_AI_CONFIG', 'DOIMATKHAU', 'F_NV_PHEDUYET'
        ))
    ) LOOP
        UPDATE HR.TB_SYS_RIGHT_CHANNEL
        SET CAN_PRINT = 1
        WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'WEB' AND FUNCTION_CODE = r.FCODE;

        IF SQL%ROWCOUNT = 0 THEN
            INSERT INTO HR.TB_SYS_RIGHT_CHANNEL (IDUSER, CLIENT_TYPE, FUNCTION_CODE, CAN_VIEW, CAN_ADD, CAN_EDIT, CAN_DELETE, CAN_PRINT)
            VALUES (v_admin_id, 'WEB', r.FCODE, 1, 1, 1, 1, 1);
        END IF;
        v_updated := v_updated + 1;
    END LOOP;

    DBMS_OUTPUT.PUT_LINE('-> Da cap nhat / dong bo CAN_PRINT=1 cho ' || v_updated || ' chuc nang tren kenh WEB.');
END;
/

PROMPT [4/5] Tang TOKEN_VERSION cua tai khoan ADMIN de buoc refresh token...
DECLARE
    v_admin_id NUMBER;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    UPDATE HR.TB_SYS_USER
       SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1
     WHERE IDUSER = v_admin_id;

    DBMS_OUTPUT.PUT_LINE('-> Da bump TOKEN_VERSION cho ADMIN.');
END;
/

PROMPT [5/5] Kiem tra va xac thuc so luong sau khi cap nhat...
DECLARE
    v_admin_id NUMBER;
    v_dt_print NUMBER;
    v_web_print NUMBER;
    v_mob_print NUMBER;
    v_dt_view NUMBER;
    v_web_view NUMBER;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    SELECT COUNT(*) INTO v_dt_print
    FROM HR.TB_SYS_RIGHT_CHANNEL
    WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'DESKTOP' AND CAN_PRINT = 1;

    SELECT COUNT(*) INTO v_web_print
    FROM HR.TB_SYS_RIGHT_CHANNEL
    WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'WEB' AND CAN_PRINT = 1;

    SELECT COUNT(*) INTO v_mob_print
    FROM HR.TB_SYS_RIGHT_CHANNEL
    WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'MOBILE' AND CAN_PRINT = 1;

    SELECT COUNT(*) INTO v_dt_view
    FROM HR.TB_SYS_RIGHT_CHANNEL
    WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'DESKTOP' AND CAN_VIEW = 1;

    SELECT COUNT(*) INTO v_web_view
    FROM HR.TB_SYS_RIGHT_CHANNEL
    WHERE IDUSER = v_admin_id AND CLIENT_TYPE = 'WEB' AND CAN_VIEW = 1;

    DBMS_OUTPUT.PUT_LINE('====================================================');
    DBMS_OUTPUT.PUT_LINE('KET QUA XAC MINH QUYEN IN VA XEM CUA ADMIN:');
    DBMS_OUTPUT.PUT_LINE('- DESKTOP CAN_PRINT = 1: ' || v_dt_print || ' / 46 chuc nang.');
    DBMS_OUTPUT.PUT_LINE('- DESKTOP CAN_VIEW  = 1: ' || v_dt_view || ' chuc nang.');
    DBMS_OUTPUT.PUT_LINE('- WEB     CAN_PRINT = 1: ' || v_web_print || ' / 34 chuc nang.');
    DBMS_OUTPUT.PUT_LINE('- WEB     CAN_VIEW  = 1: ' || v_web_view || ' chuc nang.');
    DBMS_OUTPUT.PUT_LINE('- MOBILE  CAN_PRINT = 1: ' || v_mob_print || ' (Dung chinh sach: 0).');
    DBMS_OUTPUT.PUT_LINE('====================================================');

    IF v_dt_print < 46 OR v_web_print < 34 THEN
        DBMS_OUTPUT.PUT_LINE('CANH BAO: So luong quyen In duoc cap nho hon catalogue. Vui long kiem tra lai.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('XAC NHAN: ADMIN da duoc dong bo day du quyen In theo ChannelCapabilityRegistry.');
        DBMS_OUTPUT.PUT_LINE('Luu y: Tong so thao tac can duoc danh gia toan dien qua resolver dua tren ca 5 cot thao tac.');
    END IF;
END;
/

COMMIT;
PROMPT Hoan tat Script V1_30.
EXIT;
