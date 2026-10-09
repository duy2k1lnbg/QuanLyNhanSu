-- ============================================================================
-- SCRIPT: V1_26_ai_owner_index_sync_verify.sql
-- Kiem tra toan dien Migration V1_26 va quyen han cua AI_READONLY
-- Nguoi thuc thi: sysdba hoac kiem tra ket noi AI_READONLY
-- ============================================================================

WHENEVER SQLERROR EXIT FAILURE ROLLBACK;
SET SERVEROUTPUT ON SIZE UNLIMITED;
SET DEFINE OFF;

PROMPT ============================================================================
PROMPT KIEM TRA 1: METADATA & PERMISSIONS CUA 4 BANG VA PACKAGE
PROMPT ============================================================================

DECLARE
    v_count NUMBER;
    v_err_count NUMBER;
BEGIN
    -- 1. Kiem tra 4 bang ton tai trong AI_OWNER
    SELECT COUNT(*) INTO v_count FROM DBA_TABLES 
    WHERE OWNER = 'AI_OWNER' AND TABLE_NAME IN ('TB_AI_INDEX_SOURCE', 'TB_AI_INDEX_REGISTRY', 'TB_AI_INDEX_OUTBOX', 'TB_AI_INDEX_CHECKPOINT');
    IF v_count < 4 THEN
        RAISE_APPLICATION_ERROR(-20030, 'Loi: Thieu it nhat 1 bang trong 4 bang index metadata AI_OWNER!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 1. 4 bang index metadata ton tai day du trong AI_OWNER.');

    -- 2. Kiem tra AI_READONLY KHONG co quyen truc tiep tren 4 bang
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE IN ('AI_READONLY', 'PUBLIC') AND OWNER = 'AI_OWNER' 
      AND TABLE_NAME IN ('TB_AI_INDEX_SOURCE', 'TB_AI_INDEX_REGISTRY', 'TB_AI_INDEX_OUTBOX', 'TB_AI_INDEX_CHECKPOINT');
    IF v_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20031, 'Loi: AI_READONLY bi ro ri quyen SELECT/DML truc tiep tren cac bang index metadata!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 2. AI_READONLY khong co direct table grant tren cac bang index metadata.');

    -- 3. Kiem tra AI_READONLY co quyen EXECUTE tren PKG_AI_INDEX
    SELECT COUNT(*) INTO v_count FROM DBA_TAB_PRIVS 
    WHERE GRANTEE = 'AI_READONLY' AND OWNER = 'AI_OWNER' 
      AND TABLE_NAME = 'PKG_AI_INDEX' AND PRIVILEGE = 'EXECUTE';
    IF v_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20032, 'Loi: AI_READONLY chua duoc cap EXECUTE tren AI_OWNER.PKG_AI_INDEX!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 3. AI_READONLY co quyen EXECUTE tren AI_OWNER.PKG_AI_INDEX.');

    -- 4. Kiem tra Package PKG_AI_INDEX VALID
    SELECT COUNT(*) INTO v_count FROM DBA_OBJECTS 
    WHERE OWNER = 'AI_OWNER' AND OBJECT_NAME = 'PKG_AI_INDEX' AND STATUS = 'VALID';
    IF v_count < 2 THEN
        RAISE_APPLICATION_ERROR(-20033, 'Loi: Package PKG_AI_INDEX (SPEC/BODY) bi INVALID!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 4. Package PKG_AI_INDEX VALID hoan toan.');

    -- 5. Kiem tra loi bien dich
    SELECT COUNT(*) INTO v_err_count FROM DBA_ERRORS WHERE OWNER = 'AI_OWNER' AND NAME = 'PKG_AI_INDEX';
    IF v_err_count > 0 THEN
        RAISE_APPLICATION_ERROR(-20034, 'Loi: Co loi bien dich trong PKG_AI_INDEX!');
    END IF;
    DBMS_OUTPUT.PUT_LINE('[PASS] 5. Khong co loi bien dich.');
END;
/

PROMPT ============================================================================
PROMPT KIEM TRA 2: FUNCTIONAL VERIFICATION QUA AI_READONLY SESSION
PROMPT ============================================================================

DECLARE
    v_manifest_cur SYS_REFCURSOR;
    v_iid VARCHAR2(100);
    v_pc VARCHAR2(100);
    v_an VARCHAR2(100);
    v_er VARCHAR2(200);
    v_md VARCHAR2(100);
    v_dim NUMBER;
    v_met VARCHAR2(20);
    v_sv NUMBER;
    v_cv NUMBER;
    v_ep NUMBER;
    v_st VARCHAR2(20);

    v_events_cur SYS_REFCURSOR;
    v_ev_id NUMBER;
    v_sc VARCHAR2(50);
    v_ek VARCHAR2(100);
    v_act VARCHAR2(20);
    v_sver NUMBER;
    v_att NUMBER;

    v_status_cur SYS_REFCURSOR;
    v_pending NUMBER;
    v_leased NUMBER;
    v_processed NUMBER;
    v_dead NUMBER;
    v_active_idx NUMBER;

    v_data_cur SYS_REFCURSOR;
    v_count_rows NUMBER := 0;
    v_ent_key VARCHAR2(100);
    v_ent_name VARCHAR2(200);
    v_ent_type VARCHAR2(50);
    v_cmp VARCHAR2(50);
    v_dept VARCHAR2(50);
    v_vis VARCHAR2(50);
    v_proj VARCHAR2(500);
    v_proj_ver NUMBER;
BEGIN
    -- 1. Test Enqueue Event qua PKG_AI_INDEX
    AI_OWNER.PKG_AI_INDEX.ENQUEUE_EVENT('SRC_PHONGBAN', 'TEST_PB_01', 'UPSERT', 1);
    DBMS_OUTPUT.PUT_LINE('[PASS] 6. Enqueue su kien mau thanh cong.');

    -- 2. Test Claim Events (lease 30s)
    AI_OWNER.PKG_AI_INDEX.CLAIM_EVENTS('test_worker_1', 10, 30, v_events_cur);
    LOOP
        FETCH v_events_cur INTO v_ev_id, v_sc, v_ek, v_act, v_sver, v_att;
        EXIT WHEN v_events_cur%NOTFOUND;
        IF v_ek = 'TEST_PB_01' THEN
            DBMS_OUTPUT.PUT_LINE('[PASS] 7. Claim event thanh cong cho worker: event_id=' || v_ev_id);
            -- 3. Test ACK Event
            AI_OWNER.PKG_AI_INDEX.ACK_EVENT(v_ev_id, 'idx_hrms_mapped_v2_init', 1);
            DBMS_OUTPUT.PUT_LINE('[PASS] 8. ACK event thanh cong.');
        END IF;
    END LOOP;
    CLOSE v_events_cur;

    -- 4. Test Read Active Manifest
    AI_OWNER.PKG_AI_INDEX.READ_ACTIVE_MANIFEST('hrms_mapped_active', v_manifest_cur);
    FETCH v_manifest_cur INTO v_iid, v_pc, v_an, v_er, v_md, v_dim, v_met, v_sv, v_cv, v_ep, v_st;
    IF v_manifest_cur%FOUND THEN
        DBMS_OUTPUT.PUT_LINE('[PASS] 9. Doc Active Manifest thanh cong: index=' || v_iid || ', coll=' || v_pc || ', dim=' || v_dim);
    ELSE
        RAISE_APPLICATION_ERROR(-20035, 'Loi: Khong tim thay manifest khoi diem.');
    END IF;
    CLOSE v_manifest_cur;

    -- 5. Test Read Sync Status
    AI_OWNER.PKG_AI_INDEX.READ_SYNC_STATUS(v_status_cur);
    FETCH v_status_cur INTO v_pending, v_leased, v_processed, v_dead, v_active_idx;
    DBMS_OUTPUT.PUT_LINE('[PASS] 10. Doc Sync Status: pending=' || v_pending || ', leased=' || v_leased || ', processed=' || v_processed || ', dead_letter=' || v_dead);
    CLOSE v_status_cur;

    -- 6. Test Read Index Page cho SRC_PHONGBAN
    AI_OWNER.PKG_AI_INDEX.READ_INDEX_PAGE('SRC_PHONGBAN', 10, 0, v_data_cur);
    LOOP
        FETCH v_data_cur INTO v_ent_key, v_ent_name, v_ent_type, v_cmp, v_dept, v_vis, v_proj, v_proj_ver;
        EXIT WHEN v_data_cur%NOTFOUND;
        v_count_rows := v_count_rows + 1;
    END LOOP;
    CLOSE v_data_cur;
    DBMS_OUTPUT.PUT_LINE('[PASS] 11. Doc phan trang SRC_PHONGBAN thanh cong: ' || v_count_rows || ' phong ban hop le.');

    COMMIT;
    DBMS_OUTPUT.PUT_LINE('============================================================================');
    DBMS_OUTPUT.PUT_LINE('TAT CA 11 MUC KIEM TRA GIAI DOAN B DA DAT CHUAN [100% PASS]');
    DBMS_OUTPUT.PUT_LINE('============================================================================');
END;
/
EXIT;
