-- ==============================================================================
-- HRMS: KICH BAN XOA DU LIEU LUONG VA RESET TRANG THAI CAC KY CONG
-- Muc dich:
--   1. Xoa toan bo bang luong va cac bang chi tiet luong cua tat ca ky cong.
--   2. Xoa cac lan tinh luong (Payroll Calculation Runs).
--   3. Xoa cac ky cong bat thuong (nam < 2000 hoac nam > 2100, vi du 12/9999, 10/8888).
--   4. Reset trang thai cac ky cong ve mo (KHOA = 0, LUONG_CONGBO_LOCKED = 0)
--      de san sang tinh luong va cong bo luong lai tu dau.
--
-- Cach su dung qua SQL*Plus:
--   sqlplus HR/hr@localhost:1521/orcl @database/reset_and_clean_payroll.sql
--   (Hoac localhost:1521/XEPDB1 tuy service tren may / VPS)
-- ==============================================================================

SET SERVEROUTPUT ON SIZE UNLIMITED;
SET DEFINE OFF;
SET LINESIZE 200;
SET PAGESIZE 100;

PROMPT ==============================================================================
PROMPT BAT DAU TIEN TRINH XOA LUONG VA RESET TRANG THAI KY CONG...
PROMPT ==============================================================================

-- 1. Tam thoi vo hieu hoa cac trigger chan khoa ky cong va khoa bang luong
DECLARE
    PROCEDURE toggle_trg(p_name VARCHAR2, p_action VARCHAR2) IS
        v_cnt NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cnt FROM USER_TRIGGERS WHERE TRIGGER_NAME = p_name;
        IF v_cnt > 0 THEN
            EXECUTE IMMEDIATE 'ALTER TRIGGER ' || p_name || ' ' || p_action;
        END IF;
    END;
BEGIN
    toggle_trg('TRG45_PERIOD_FROZEN', 'DISABLE');
    toggle_trg('TRG45_PAYROLL_FROZEN', 'DISABLE');
    toggle_trg('TRG45_FREEZE_01', 'DISABLE');
    toggle_trg('TRG45_FREEZE_02', 'DISABLE');
    toggle_trg('TRG45_FREEZE_03', 'DISABLE');
    toggle_trg('TRG45_FREEZE_04', 'DISABLE');
    toggle_trg('TRG45_FREEZE_05', 'DISABLE');
    toggle_trg('TRG45_FREEZE_06', 'DISABLE');
    toggle_trg('TRG45_FREEZE_07', 'DISABLE');
    toggle_trg('TRG45_FREEZE_08', 'DISABLE');
    toggle_trg('TRG45_FREEZE_09', 'DISABLE');
    DBMS_OUTPUT.PUT_LINE('1. Da tam thoi DISABLE cac trigger khoa bang luong / ky cong.');
END;
/

-- 2. Xoa toan bo du lieu bang luong va cac bang chi tiet luong
DECLARE
    PROCEDURE clear_table(p_table VARCHAR2) IS
        v_cnt NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cnt FROM USER_TABLES WHERE TABLE_NAME = p_table;
        IF v_cnt > 0 THEN
            EXECUTE IMMEDIATE 'DELETE FROM ' || p_table;
            DBMS_OUTPUT.PUT_LINE('   - Da xoa bang: ' || RPAD(p_table, 30) || ' (' || SQL%ROWCOUNT || ' dong)');
        END IF;
    END;
BEGIN
    DBMS_OUTPUT.PUT_LINE('2. Xoa toan bo du lieu bang luong va chi tiet luong:');
    clear_table('TB_BANGLUONG_CT_SOURCE');
    clear_table('TB_BANGLUONG_CT');
    clear_table('TB_BANGLUONG_BAOHIEM');
    clear_table('TB_BANGLUONG_CONG_DOAN');
    clear_table('TB_BANGLUONG_THUE_CT');
    clear_table('TB_BANGLUONG_OT_COMPLIANCE');
    clear_table('TB_QUYET_TOAN_THUE_NAM_CT');
    clear_table('TB_QUYET_TOAN_THUE_NAM');
    clear_table('TB_BANGLUONG');
    COMMIT;
END;
/

-- 3. Xoa cac lan tinh luong (Payroll Calculation Runs)
DECLARE
    v_cnt NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_cnt FROM USER_TABLES WHERE TABLE_NAME = 'TB_PAYROLL_CALCULATION_RUN';
    IF v_cnt > 0 THEN
        EXECUTE IMMEDIATE 'DELETE FROM TB_PAYROLL_CALCULATION_RUN';
        DBMS_OUTPUT.PUT_LINE('3. Da xoa cac lan tinh luong trong TB_PAYROLL_CALCULATION_RUN (' || SQL%ROWCOUNT || ' dong).');
        COMMIT;
    ELSE
        DBMS_OUTPUT.PUT_LINE('3. Bang TB_PAYROLL_CALCULATION_RUN khong ton tai (bo qua).');
    END IF;
END;
/

-- 4. Xoa cac ky cong bat thuong (nam < 2000 hoac nam > 2100, vi du: 12/9999, 10/8888)
DECLARE
    v_deleted NUMBER := 0;
BEGIN
    -- Xoa du lieu lien quan neu co
    DELETE FROM TB_KYCONGCHITIET WHERE MAKYCONG IN (SELECT MAKYCONG FROM TB_KYCONG WHERE NAM < 2000 OR NAM > 2100);
    DELETE FROM TB_BANGCONG_CHITIET WHERE MAKYCONG IN (SELECT MAKYCONG FROM TB_KYCONG WHERE NAM < 2000 OR NAM > 2100);
    DELETE FROM TB_CHAMCONG_KQ_NGAY WHERE MAKYCONG IN (SELECT MAKYCONG FROM TB_KYCONG WHERE NAM < 2000 OR NAM > 2100);
    DELETE FROM TB_CHAMCONG_LANTINH WHERE MAKYCONG IN (SELECT MAKYCONG FROM TB_KYCONG WHERE NAM < 2000 OR NAM > 2100);
    DELETE FROM TB_BAOHIEM_BIENDONG WHERE MAKYCONG IN (SELECT MAKYCONG FROM TB_KYCONG WHERE NAM < 2000 OR NAM > 2100);
    DELETE FROM TB_BANGCONG WHERE NAM < 2000 OR NAM > 2100;
    DELETE FROM TB_TANGCA WHERE NAM < 2000 OR NAM > 2100;
    DELETE FROM TB_UNGLUONG WHERE NAM < 2000 OR NAM > 2100;
    DELETE FROM TB_YEUCAU_UNGLUONG WHERE NAM < 2000 OR NAM > 2100;
    DELETE FROM TB_KHENTHUONG_KYLUAT WHERE NAM_APDUNG < 2000 OR NAM_APDUNG > 2100;

    -- Xoa truc tiep khoi TB_KYCONG
    DELETE FROM TB_KYCONG WHERE NAM < 2000 OR NAM > 2100;
    v_deleted := SQL%ROWCOUNT;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('4. Da xoa cac ky cong ngoai khoang 2000-2100 (vi du 12/9999, 10/8888): ' || v_deleted || ' ky.');
END;
/

-- 5. Reset trang thai tat ca ky cong con lai (KHOA = 0, LUONG_CONGBO_LOCKED = 0)
DECLARE
    v_updated NUMBER := 0;
BEGIN
    UPDATE TB_KYCONG
    SET KHOA = 0,
        LUONG_CONGBO_LOCKED = 0;
    v_updated := SQL%ROWCOUNT;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('5. Da reset KHOA=0 va LUONG_CONGBO_LOCKED=0 cho ' || v_updated || ' ky cong con lai.');
END;
/

-- 6. Bat lai cac trigger chan khoa
DECLARE
    PROCEDURE toggle_trg(p_name VARCHAR2, p_action VARCHAR2) IS
        v_cnt NUMBER;
    BEGIN
        SELECT COUNT(*) INTO v_cnt FROM USER_TRIGGERS WHERE TRIGGER_NAME = p_name;
        IF v_cnt > 0 THEN
            EXECUTE IMMEDIATE 'ALTER TRIGGER ' || p_name || ' ' || p_action;
        END IF;
    END;
BEGIN
    toggle_trg('TRG45_PERIOD_FROZEN', 'ENABLE');
    toggle_trg('TRG45_PAYROLL_FROZEN', 'ENABLE');
    toggle_trg('TRG45_FREEZE_01', 'ENABLE');
    toggle_trg('TRG45_FREEZE_02', 'ENABLE');
    toggle_trg('TRG45_FREEZE_03', 'ENABLE');
    toggle_trg('TRG45_FREEZE_04', 'ENABLE');
    toggle_trg('TRG45_FREEZE_05', 'ENABLE');
    toggle_trg('TRG45_FREEZE_06', 'ENABLE');
    toggle_trg('TRG45_FREEZE_07', 'ENABLE');
    toggle_trg('TRG45_FREEZE_08', 'ENABLE');
    toggle_trg('TRG45_FREEZE_09', 'ENABLE');
    DBMS_OUTPUT.PUT_LINE('6. Da ENABLE lai toan bo cac trigger bao ve tinh toan ven.');
END;
/

PROMPT ==============================================================================
PROMPT KIEM TRA SAU KHI RESET:
PROMPT ==============================================================================

PROMPT Danh sach cac ky cong hien co:
SELECT MAKYCONG, THANG, NAM, KHOA, TRANGTHAI, LUONG_CONGBO_LOCKED
FROM TB_KYCONG
ORDER BY MAKYCONG;

PROMPT So luong ban ghi luong con lai (mong muon: 0):
SELECT COUNT(*) AS TONG_SO_BANGLUONG FROM TB_BANGLUONG;

PROMPT HOAN TAT RESET LUONG VA KY CONG THANH CONG!
EXIT;
