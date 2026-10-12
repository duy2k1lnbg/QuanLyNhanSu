using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Bu.CLASS_SYSTEM
{
    public class DeviceRegisterDto
    {
        public string InstallationId { get; set; }
        public string ExpoPushToken { get; set; }
        public string Platform { get; set; }
        public string ProjectId { get; set; }
        public string Environment { get; set; }
        public string Language { get; set; }
        public string AppVersion { get; set; }
        public string PermissionStatus { get; set; }
        public bool? PushEnabled { get; set; }
        public string DeviceName { get; set; }
    }

    public class DevicePreferenceDto
    {
        public bool? PushEnabled { get; set; }
        public string Language { get; set; }
        public string PermissionStatus { get; set; }
    }

    public class UserDeviceInfoDto
    {
        public long Id { get; set; }
        public string InstallationId { get; set; }
        public string Platform { get; set; }
        public string DeviceName { get; set; }
        public bool PushEnabled { get; set; }
        public string PermissionStatus { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ActiveDeviceTarget
    {
        public long DeviceId { get; set; }
        public long IdUser { get; set; }
        public long? Manv { get; set; }
        public string InstallationId { get; set; }
        public string ExpoPushToken { get; set; }
        public string Language { get; set; }
    }

    public static class DeviceRegistrationService
    {
        private static string GetConnectionString()
        {
            using (var db = new MyEntities())
            {
                return Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
            }
        }

        public static bool RegisterOrUpdateDevice(long userId, long? manv, DeviceRegisterDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.InstallationId))
            {
                throw new ArgumentException("InstallationId không được để trống.");
            }

            string platform = (dto.Platform ?? "UNKNOWN").ToUpperInvariant();
            if (platform.Length > 30) platform = platform.Substring(0, 30);

            string permStatus = (dto.PermissionStatus ?? "UNDETERMINED").ToUpperInvariant();
            if (permStatus != "GRANTED" && permStatus != "DENIED") permStatus = "UNDETERMINED";

            string env = (dto.Environment ?? "PRODUCTION").ToUpperInvariant();
            if (env.Length > 30) env = env.Substring(0, 30);

            string lang = string.IsNullOrWhiteSpace(dto.Language) ? "vi" : dto.Language.Trim();
            if (lang.Length > 10) lang = lang.Substring(0, 10);

            int pushEnabled = (dto.PushEnabled.HasValue && !dto.PushEnabled.Value) ? 0 : 1;
            string installId = dto.InstallationId.Trim();
            if (installId.Length > 100) installId = installId.Substring(0, 100);

            string token = string.IsNullOrWhiteSpace(dto.ExpoPushToken) ? null : dto.ExpoPushToken.Trim();
            if (token != null && token.Length > 255) token = token.Substring(0, 255);

            string deviceName = string.IsNullOrWhiteSpace(dto.DeviceName) ? null : dto.DeviceName.Trim();
            if (deviceName != null && deviceName.Length > 150) deviceName = deviceName.Substring(0, 150);

            string appVer = string.IsNullOrWhiteSpace(dto.AppVersion) ? null : dto.AppVersion.Trim();
            if (appVer != null && appVer.Length > 50) appVer = appVer.Substring(0, 50);

            string projId = string.IsNullOrWhiteSpace(dto.ProjectId) ? null : dto.ProjectId.Trim();
            if (projId != null && projId.Length > 100) projId = projId.Substring(0, 100);

            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string mergeSql = @"
                    MERGE INTO TB_USER_DEVICE tgt
                    USING (SELECT :p_install AS INSTALLATION_ID FROM DUAL) src
                    ON (tgt.INSTALLATION_ID = src.INSTALLATION_ID)
                    WHEN MATCHED THEN
                        UPDATE SET
                            IDUSER = :p_uid,
                            MANV = :p_manv,
                            EXPO_PUSH_TOKEN = :p_token,
                            PLATFORM = :p_platform,
                            PROJECT_ID = :p_proj,
                            ENVIRONMENT = :p_env,
                            LANGUAGE = :p_lang,
                            APP_VERSION = :p_ver,
                            PERMISSION_STATUS = :p_perm,
                            PUSH_ENABLED = :p_push_en,
                            IS_ACTIVE = 1,
                            DEVICE_NAME = :p_dname,
                            UPDATED_AT = SYSDATE,
                            LAST_SEEN_AT = SYSDATE
                    WHEN NOT MATCHED THEN
                        INSERT (
                            ID, IDUSER, MANV, INSTALLATION_ID, EXPO_PUSH_TOKEN, PLATFORM, PROJECT_ID,
                            ENVIRONMENT, LANGUAGE, APP_VERSION, PERMISSION_STATUS, PUSH_ENABLED, IS_ACTIVE,
                            DEVICE_NAME, CREATED_AT, UPDATED_AT, LAST_SEEN_AT
                        ) VALUES (
                            SEQ_USER_DEVICE.NEXTVAL, :p_uid, :p_manv, :p_install, :p_token, :p_platform, :p_proj,
                            :p_env, :p_lang, :p_ver, :p_perm, :p_push_en, 1,
                            :p_dname, SYSDATE, SYSDATE, SYSDATE
                        )";

                using (var cmd = new OracleCommand(mergeSql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_install", installId);
                    cmd.Parameters.Add("p_uid", userId);
                    cmd.Parameters.Add("p_manv", (object)manv ?? DBNull.Value);
                    cmd.Parameters.Add("p_token", (object)token ?? DBNull.Value);
                    cmd.Parameters.Add("p_platform", platform);
                    cmd.Parameters.Add("p_proj", (object)projId ?? DBNull.Value);
                    cmd.Parameters.Add("p_env", env);
                    cmd.Parameters.Add("p_lang", lang);
                    cmd.Parameters.Add("p_ver", (object)appVer ?? DBNull.Value);
                    cmd.Parameters.Add("p_perm", permStatus);
                    cmd.Parameters.Add("p_push_en", pushEnabled);
                    cmd.Parameters.Add("p_dname", (object)deviceName ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        public static bool UpdatePreferences(long userId, string installationId, DevicePreferenceDto dto)
        {
            if (string.IsNullOrWhiteSpace(installationId) || dto == null) return false;

            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();

                string permStatus = null;
                if (!string.IsNullOrWhiteSpace(dto.PermissionStatus))
                {
                    string p = dto.PermissionStatus.ToUpperInvariant();
                    if (p == "GRANTED" || p == "DENIED" || p == "UNDETERMINED")
                        permStatus = p;
                }

                string lang = null;
                if (!string.IsNullOrWhiteSpace(dto.Language))
                {
                    lang = dto.Language.Trim();
                    if (lang.Length > 10) lang = lang.Substring(0, 10);
                }

                int? pushEn = null;
                if (dto.PushEnabled.HasValue)
                {
                    pushEn = dto.PushEnabled.Value ? 1 : 0;
                }

                string sql = @"
                    UPDATE TB_USER_DEVICE
                    SET PUSH_ENABLED = NVL(:p_push_en, PUSH_ENABLED),
                        LANGUAGE = NVL(:p_lang, LANGUAGE),
                        PERMISSION_STATUS = NVL(:p_perm, PERMISSION_STATUS),
                        UPDATED_AT = SYSDATE,
                        LAST_SEEN_AT = SYSDATE
                    WHERE INSTALLATION_ID = :p_install AND IDUSER = :p_uid";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_push_en", (object)pushEn ?? DBNull.Value);
                    cmd.Parameters.Add("p_lang", (object)lang ?? DBNull.Value);
                    cmd.Parameters.Add("p_perm", (object)permStatus ?? DBNull.Value);
                    cmd.Parameters.Add("p_install", installationId.Trim());
                    cmd.Parameters.Add("p_uid", userId);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }

        public static bool DeactivateDevice(long userId, string installationId)
        {
            if (string.IsNullOrWhiteSpace(installationId)) return false;

            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    UPDATE TB_USER_DEVICE
                    SET IS_ACTIVE = 0,
                        UPDATED_AT = SYSDATE,
                        LAST_SEEN_AT = SYSDATE
                    WHERE INSTALLATION_ID = :p_install AND IDUSER = :p_uid";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_install", installationId.Trim());
                    cmd.Parameters.Add("p_uid", userId);

                    int affected = cmd.ExecuteNonQuery();
                    return affected > 0;
                }
            }
        }

        public static List<UserDeviceInfoDto> GetDevicesForUser(long userId)
        {
            var list = new List<UserDeviceInfoDto>();
            string connStr = GetConnectionString();
            using (var conn = new OracleConnection(connStr))
            {
                conn.Open();
                string sql = @"
                    SELECT ID, INSTALLATION_ID, PLATFORM, DEVICE_NAME, PUSH_ENABLED, PERMISSION_STATUS, IS_ACTIVE, LAST_SEEN_AT, CREATED_AT
                    FROM TB_USER_DEVICE
                    WHERE IDUSER = :p_uid
                    ORDER BY LAST_SEEN_AT DESC";

                using (var cmd = new OracleCommand(sql, conn))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add("p_uid", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new UserDeviceInfoDto
                            {
                                Id = Convert.ToInt64(reader["ID"]),
                                InstallationId = reader["INSTALLATION_ID"]?.ToString(),
                                Platform = reader["PLATFORM"]?.ToString(),
                                DeviceName = reader["DEVICE_NAME"]?.ToString(),
                                PushEnabled = Convert.ToInt32(reader["PUSH_ENABLED"]) == 1,
                                PermissionStatus = reader["PERMISSION_STATUS"]?.ToString(),
                                IsActive = Convert.ToInt32(reader["IS_ACTIVE"]) == 1,
                                LastSeenAt = reader["LAST_SEEN_AT"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["LAST_SEEN_AT"]) : null,
                                CreatedAt = reader["CREATED_AT"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["CREATED_AT"]) : null
                            });
                        }
                    }
                }
            }
            return list;
        }

        public static List<ActiveDeviceTarget> GetActiveDevicesForTarget(
            string targetType,
            long? targetManv,
            string targetPb,
            string targetCty,
            OracleConnection conn,
            OracleTransaction tx = null)
        {
            var results = new List<ActiveDeviceTarget>();
            string normTargetType = (targetType ?? "ALL").ToUpperInvariant();

            string sql = @"
                SELECT d.ID, d.IDUSER, d.MANV, d.INSTALLATION_ID, d.EXPO_PUSH_TOKEN, d.LANGUAGE
                FROM TB_USER_DEVICE d
                WHERE d.IS_ACTIVE = 1
                  AND d.PUSH_ENABLED = 1
                  AND d.EXPO_PUSH_TOKEN IS NOT NULL
                  AND d.PERMISSION_STATUS = 'GRANTED'
                  AND (
                    (:p_ttype = 'PERSONAL' AND :p_manv IS NOT NULL AND (d.MANV = :p_manv OR d.IDUSER IN (
                        SELECT u.IDUSER FROM TB_SYS_USER u WHERE u.MANV = :p_manv
                        UNION
                        SELECT m.USER_ID FROM TB_USER_EMPLOYEE_MAPPING m WHERE m.EMPLOYEE_ID = :p_manv
                    )))
                    OR
                    (:p_ttype = 'DEPARTMENT' AND :p_pb IS NOT NULL AND d.MANV IN (
                        SELECT nv.MANV FROM TB_NHANVIEN nv 
                        WHERE TO_CHAR(nv.IDPB) = :p_pb
                          AND (:p_cty IS NULL OR TO_CHAR(nv.IDCTY) = :p_cty)
                    ))
                    OR
                    (:p_ttype = 'COMPANY' AND :p_cty IS NOT NULL AND d.MANV IN (
                        SELECT nv.MANV FROM TB_NHANVIEN nv
                        WHERE TO_CHAR(nv.IDCTY) = :p_cty
                    ))
                    OR
                    (:p_ttype = 'ALL' AND (
                        :p_cty IS NULL OR d.MANV IN (
                            SELECT nv.MANV FROM TB_NHANVIEN nv WHERE TO_CHAR(nv.IDCTY) = :p_cty
                        )
                    ))
                  )";

            using (var cmd = new OracleCommand(sql, conn))
            {
                if (tx != null) cmd.Transaction = tx;
                cmd.BindByName = true;
                cmd.Parameters.Add("p_ttype", normTargetType);
                cmd.Parameters.Add("p_manv", (object)targetManv ?? DBNull.Value);
                cmd.Parameters.Add("p_pb", string.IsNullOrWhiteSpace(targetPb) ? (object)DBNull.Value : targetPb.Trim());
                cmd.Parameters.Add("p_cty", string.IsNullOrWhiteSpace(targetCty) ? (object)DBNull.Value : targetCty.Trim());

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ActiveDeviceTarget
                        {
                            DeviceId = Convert.ToInt64(reader["ID"]),
                            IdUser = Convert.ToInt64(reader["IDUSER"]),
                            Manv = reader["MANV"] != DBNull.Value ? (long?)Convert.ToInt64(reader["MANV"]) : null,
                            InstallationId = reader["INSTALLATION_ID"]?.ToString(),
                            ExpoPushToken = reader["EXPO_PUSH_TOKEN"]?.ToString(),
                            Language = reader["LANGUAGE"]?.ToString() ?? "vi"
                        });
                    }
                }
            }

            return results;
        }

        public static void DeactivateToken(string expoToken, string reason)
        {
            if (string.IsNullOrWhiteSpace(expoToken)) return;

            try
            {
                string connStr = GetConnectionString();
                using (var conn = new OracleConnection(connStr))
                {
                    conn.Open();
                    string sql = @"
                        UPDATE TB_USER_DEVICE
                        SET IS_ACTIVE = 0,
                            PERMISSION_STATUS = 'DENIED',
                            UPDATED_AT = SYSDATE
                        WHERE EXPO_PUSH_TOKEN = :p_token";

                    using (var cmd = new OracleCommand(sql, conn))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("p_token", expoToken.Trim());
                        int rows = cmd.ExecuteNonQuery();
                        System.Diagnostics.Trace.TraceInformation($"[DeviceRegistrationService.DeactivateToken]: Deactivated {rows} devices for token {expoToken}. Reason: {reason}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[DeviceRegistrationService.DeactivateToken Error]: " + ex.Message);
            }
        }
    }
}
