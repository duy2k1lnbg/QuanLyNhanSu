-- ============================================================================
-- SCRIPT: V1_30_rollback__admin_channel_print_grants.sql
-- Muc dich: Rollback quyen CAN_PRINT ve 0 cho ADMIN tren DESKTOP va WEB.
-- Nguoi chay: DBA
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;

PROMPT Rollback CAN_PRINT ve 0 cho ADMIN tren DESKTOP va WEB...
DECLARE
    v_admin_id NUMBER;
    v_updated  NUMBER := 0;
BEGIN
    SELECT IDUSER INTO v_admin_id
    FROM HR.TB_SYS_USER
    WHERE UPPER(TRIM(USERNAME)) = 'ADMIN' AND NVL(ISGROUP, 0) = 0;

    -- Rollback dung 46 chuc nang Desktop ma V1_30 da cap
    UPDATE HR.TB_SYS_RIGHT_CHANNEL
       SET CAN_PRINT = 0
     WHERE IDUSER = v_admin_id
       AND CLIENT_TYPE = 'DESKTOP'
       AND FUNCTION_CODE IN (
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
       );
    v_updated := v_updated + SQL%ROWCOUNT;

    -- Rollback dung 34 chuc nang Web ma V1_30 da cap
    UPDATE HR.TB_SYS_RIGHT_CHANNEL
       SET CAN_PRINT = 0
     WHERE IDUSER = v_admin_id
       AND CLIENT_TYPE = 'WEB'
       AND FUNCTION_CODE IN (
            'F_DM_BOPHAN', 'F_DM_CHUCVU', 'F_DM_CONGTY', 'F_DM_DANTOC', 'F_DM_NHANVIEN',
            'F_DM_PHONGBAN', 'F_DM_TONGIAO', 'F_DM_TRINHDO', 'F_DM_LOAIPHEP',
            'F_NV_LOAIHOPDONG', 'F_NV_DIEUCHUYEN', 'F_NV_HOPDONG', 'F_NV_KHENTHUONG',
            'F_NV_KYLUAT', 'F_NV_NANGLUONG', 'F_NV_THOIVIEC', 'F_NV_NGHIPHEP',
            'F_CC_BANGCONG', 'F_CC_BCCT', 'F_CC_NGAYLE',
            'F_BC_BAOCAO', 'F_DB_LUONG', 'F_DB_NHANSU',
            'F_SYSTEM_USER', 'F_SYSTEM_GROUP', 'F_SYSTEM_LOCK_USER', 'F_SYSTEM_CAPTAIKHOAN',
            'F_SYSTEM_PQ_CHUCNANG', 'F_SYSTEM_PQ_BAOCAO', 'F_SYSTEM_THONGBAO',
            'F_SYSTEM_AI', 'F_SYSTEM_AI_CONFIG', 'DOIMATKHAU', 'F_NV_PHEDUYET'
       );
    v_updated := v_updated + SQL%ROWCOUNT;

    UPDATE HR.TB_SYS_USER
       SET TOKEN_VERSION = NVL(TOKEN_VERSION, 1) + 1
     WHERE IDUSER = v_admin_id;

    DBMS_OUTPUT.PUT_LINE('-> Rollback hoan tat: Da revert CAN_PRINT=0 cho ' || v_updated || ' dong.');
END;
/

COMMIT;
PROMPT Hoan tat Rollback V1_30.
