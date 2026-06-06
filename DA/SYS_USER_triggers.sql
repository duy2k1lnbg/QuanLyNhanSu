-- Oracle Trigger to handle cascade deletes for users and groups in system tables
-- Run this script in your Oracle database client (e.g. SQL Developer, PL/SQL Developer, or SQL*Plus)

CREATE OR REPLACE TRIGGER TRG_TB_SYS_USER_DELETE
BEFORE DELETE ON TB_SYS_USER
FOR EACH ROW
DECLARE
BEGIN
    -- 1. Remove memberships from groups where this user was a member
    DELETE FROM TB_SYS_GROUP WHERE MEMBER = :OLD.IDUSER;

    -- 2. Remove all members if the deleted entity was a group
    DELETE FROM TB_SYS_GROUP WHERE ID_GROUP = :OLD.IDUSER;

    -- 3. Remove function rights associated with this user/group
    DELETE FROM TB_SYS_RIGHT WHERE IDUSER = :OLD.IDUSER;

    -- 4. Remove report rights associated with this user/group
    DELETE FROM TB_SYS_RIGHT_REPORT WHERE IDUSER = :OLD.IDUSER;
END;
/
