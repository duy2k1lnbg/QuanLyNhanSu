-- Read group membership through a proof-validated AI_OWNER procedure.
-- This does not grant AI_READONLY access to HR tables or HR views.
CREATE OR REPLACE PROCEDURE AI_OWNER.GET_ACTOR_ROLE_IDS (
    p_actor_id IN NUMBER, p_audience IN VARCHAR2, p_nonce IN VARCHAR2,
    p_exp IN NUMBER, p_signature IN VARCHAR2, p_roles OUT SYS_REFCURSOR
) AUTHID DEFINER AS
    c_user SYS_REFCURSOR; c_rights SYS_REFCURSOR; c_caps SYS_REFCURSOR;
    c_fields SYS_REFCURSOR; c_grants SYS_REFCURSOR; c_revs SYS_REFCURSOR;
BEGIN
    -- GET_ACTOR_SNAPSHOT validates HMAC proof, audience, expiry, nonce and actor.
    AI_OWNER.PKG_AI_AUTH.GET_ACTOR_SNAPSHOT(
        p_actor_id, p_audience, p_nonce, p_exp, p_signature,
        c_user, c_rights, c_caps, c_fields, c_grants, c_revs);
    CLOSE c_user; CLOSE c_rights; CLOSE c_caps;
    CLOSE c_fields; CLOSE c_grants; CLOSE c_revs;
    OPEN p_roles FOR
        SELECT DISTINCT member.ID_GROUP AS ROLE_ID
        FROM HR.V_AI_SRC_AUTH member
        JOIN HR.V_AI_SRC_AUTH role_actor ON role_actor.IDUSER = member.ID_GROUP
        WHERE member.IDUSER = p_actor_id
          AND NVL(member.DISABLED, 0) = 0 AND NVL(member.ISGROUP, 0) = 0
          AND NVL(role_actor.DISABLED, 0) = 0 AND NVL(role_actor.ISGROUP, 0) = 1
          AND member.ID_GROUP IS NOT NULL;
EXCEPTION WHEN OTHERS THEN
    IF c_user%ISOPEN THEN CLOSE c_user; END IF;
    IF c_rights%ISOPEN THEN CLOSE c_rights; END IF;
    IF c_caps%ISOPEN THEN CLOSE c_caps; END IF;
    IF c_fields%ISOPEN THEN CLOSE c_fields; END IF;
    IF c_grants%ISOPEN THEN CLOSE c_grants; END IF;
    IF c_revs%ISOPEN THEN CLOSE c_revs; END IF;
    RAISE;
END;
/
GRANT EXECUTE ON AI_OWNER.GET_ACTOR_ROLE_IDS TO AI_READONLY;
-- Rollback: REVOKE EXECUTE ON AI_OWNER.GET_ACTOR_ROLE_IDS FROM AI_READONLY;
--           DROP PROCEDURE AI_OWNER.GET_ACTOR_ROLE_IDS;
