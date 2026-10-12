-- Restore old applications first. Metadata is retained to preserve audit history.
CREATE OR REPLACE TRIGGER HR.TRG_ENFORCE_RIGHT_CHANNEL
BEFORE INSERT OR UPDATE ON HR.TB_SYS_RIGHT_CHANNEL
FOR EACH ROW
BEGIN
    -- Quy tắc 1: Web Portal cấm tuyệt đối action thêm/sửa/xóa/in đối với Bảng lương
    IF :NEW.CLIENT_TYPE = 'WEB' AND :NEW.FUNCTION_CODE = 'F_CC_BANGLUONG' THEN
        :NEW.CAN_ADD := 0;
        :NEW.CAN_EDIT := 0;
        :NEW.CAN_DELETE := 0;
        :NEW.CAN_PRINT := 0;
    END IF;

    -- Quy tắc 2: Mobile App không chấp nhận các chức năng quản trị hệ thống Desktop
    IF :NEW.CLIENT_TYPE = 'MOBILE' AND :NEW.FUNCTION_CODE IN ('F_SYSTEM_DB_CONFIG', 'F_SYSTEM_SAULUU', 'F_SYSTEM_PHUCHOI', 'F_SYSTEM_SETTING') THEN
        :NEW.CAN_VIEW := 0;
        :NEW.CAN_ADD := 0;
        :NEW.CAN_EDIT := 0;
        :NEW.CAN_DELETE := 0;
        :NEW.CAN_PRINT := 0;
    END IF;

    :NEW.UPDATED_AT := SYSTIMESTAMP;
END;
/

