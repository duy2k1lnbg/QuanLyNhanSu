using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DA;
using Bu.DTO;

namespace Bu.CLASS_SYSTEM
{
    public class SYS_LANGUAGE
    {
        private readonly MyEntities db = new MyEntities();

        public SYS_LANGUAGE()
        {
            EnsureSeeded();
        }

        public void EnsureSeeded()
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }

                // Check if TB_LANGUAGES exists in Oracle
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM user_tables WHERE table_name = 'TB_LANGUAGES'";
                    int tableExists = Convert.ToInt32(cmd.ExecuteScalar());

                    if (tableExists == 0)
                    {
                        // Create Table
                        using (var createCmd = conn.CreateCommand())
                        {
                            createCmd.CommandText = @"
                                CREATE TABLE TB_LANGUAGES (
                                    code VARCHAR2(5) PRIMARY KEY,
                                    name VARCHAR2(50) NOT NULL,
                                    is_active NUMBER(1) DEFAULT 1 NOT NULL, 
                                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                    CONSTRAINT chk_lang_active CHECK (is_active IN (0, 1))
                                )";
                            createCmd.ExecuteNonQuery();
                        }

                        // Seed default values
                        string[] insertQueries = {
                            "INSERT INTO TB_LANGUAGES (code, name, is_active) VALUES ('vi', 'Tiếng Việt', 1)",
                            "INSERT INTO TB_LANGUAGES (code, name, is_active) VALUES ('en', 'Tiếng Anh', 1)",
                            "INSERT INTO TB_LANGUAGES (code, name, is_active) VALUES ('ja', 'Tiếng Nhật', 1)",
                            "INSERT INTO TB_LANGUAGES (code, name, is_active) VALUES ('zh', 'Tiếng Trung', 1)",
                            "INSERT INTO TB_LANGUAGES (code, name, is_active) VALUES ('ko', 'Tiếng Hàn', 1)"
                        };

                        foreach (var query in insertQueries)
                        {
                            using (var insertCmd = conn.CreateCommand())
                            {
                                insertCmd.CommandText = query;
                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SYS_LANGUAGE SEED ERROR]: {ex.Message}");
            }
        }

        public List<SysLanguageDTO> getList()
        {
            var list = new List<SysLanguageDTO>();
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT code, name, is_active, created_at FROM TB_LANGUAGES ORDER BY name ASC";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new SysLanguageDTO
                            {
                                CODE = reader.GetString(0),
                                NAME = reader.GetString(1),
                                IS_ACTIVE = reader.GetInt32(2) == 1,
                                CREATED_AT = reader.GetDateTime(3)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tải danh sách ngôn ngữ: " + ex.Message);
            }
            return list;
        }

        public SysLanguageDTO getItem(string code)
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT code, name, is_active, created_at FROM TB_LANGUAGES WHERE code = :code";
                    var param = cmd.CreateParameter();
                    param.ParameterName = "code";
                    param.Value = code;
                    cmd.Parameters.Add(param);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SysLanguageDTO
                            {
                                CODE = reader.GetString(0),
                                NAME = reader.GetString(1),
                                IS_ACTIVE = reader.GetInt32(2) == 1,
                                CREATED_AT = reader.GetDateTime(3)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lấy thông tin ngôn ngữ: " + ex.Message);
            }
            return null;
        }

        public void Add(SysLanguageDTO item)
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO TB_LANGUAGES (code, name, is_active) VALUES (:code, :name, :is_active)";
                    
                    var pCode = cmd.CreateParameter();
                    pCode.ParameterName = "code";
                    pCode.Value = item.CODE;
                    cmd.Parameters.Add(pCode);

                    var pName = cmd.CreateParameter();
                    pName.ParameterName = "name";
                    pName.Value = item.NAME;
                    cmd.Parameters.Add(pName);

                    var pActive = cmd.CreateParameter();
                    pActive.ParameterName = "is_active";
                    pActive.Value = item.IS_ACTIVE ? 1 : 0;
                    cmd.Parameters.Add(pActive);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm ngôn ngữ mới: " + ex.Message);
            }
        }

        public void Update(SysLanguageDTO item)
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE TB_LANGUAGES SET name = :name, is_active = :is_active WHERE code = :code";

                    var pName = cmd.CreateParameter();
                    pName.ParameterName = "name";
                    pName.Value = item.NAME;
                    cmd.Parameters.Add(pName);

                    var pActive = cmd.CreateParameter();
                    pActive.ParameterName = "is_active";
                    pActive.Value = item.IS_ACTIVE ? 1 : 0;
                    cmd.Parameters.Add(pActive);

                    var pCode = cmd.CreateParameter();
                    pCode.ParameterName = "code";
                    pCode.Value = item.CODE;
                    cmd.Parameters.Add(pCode);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật ngôn ngữ: " + ex.Message);
            }
        }

        public void Delete(string code)
        {
            try
            {
                var conn = db.Database.Connection;
                if (conn.State != ConnectionState.Open) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM TB_LANGUAGES WHERE code = :code";

                    var pCode = cmd.CreateParameter();
                    pCode.ParameterName = "code";
                    pCode.Value = code;
                    cmd.Parameters.Add(pCode);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa ngôn ngữ: " + ex.Message);
            }
        }
    }
}
