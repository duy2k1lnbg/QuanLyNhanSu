using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bu.CLASS_SYSTEM
{
    public class SYS_USER
    {
        MyEntities db = new MyEntities();

        public TB_SYS_USER getItem(int iduser)
        {
            return db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == iduser);
        }
        public List<TB_SYS_USER> getALL() 
        { 
            return db.TB_SYS_USER.ToList();
        }

        public bool checkUserExist(string username)
        {
            var us = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME == username);
            if (us != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public TB_SYS_USER Add(TB_SYS_USER user) 
        {
            try
            {
                db.TB_SYS_USER.Add(user);
                db.SaveChanges();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi tạo Group/User. " +ex.Message);
            }
        }

        public TB_SYS_USER Update(TB_SYS_USER user)
        {
            try
            {
                var entity = db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == user.IDUSER);
                if (entity != null)
                {
                    entity.FULLNAME = user.FULLNAME;
                    entity.DISABLED = user.DISABLED;
                    entity.MACTY = user.MACTY;
                    entity.MADVI = user.MADVI;
                    if (!string.IsNullOrEmpty(user.PASSWORD))
                    {
                        entity.PASSWORD = user.PASSWORD;
                    }
                    db.SaveChanges();
                }
                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi cập nhật User/Group: " + ex.Message);
            }
        }

        public void Delete(decimal idUser)
        {
            try
            {
                var entity = db.TB_SYS_USER.FirstOrDefault(x => x.IDUSER == idUser);
                if (entity != null)
                {
                    db.TB_SYS_USER.Remove(entity);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa User/Group: " + ex.Message);
            }
        }

        public List<TB_SYS_USER> GetGroupMembers(decimal idGroup)
        {
            var memberIds = db.TB_SYS_GROUP.Where(g => g.ID_GROUP == idGroup).Select(g => g.MEMBER).ToList();
            return db.TB_SYS_USER.Where(u => memberIds.Contains(u.IDUSER)).ToList();
        }

        public List<TB_SYS_USER> GetUsersNotInGroup(decimal idGroup)
        {
            var memberIds = db.TB_SYS_GROUP.Where(g => g.ID_GROUP == idGroup).Select(g => g.MEMBER).ToList();
            return db.TB_SYS_USER.Where(u => u.ISGROUP == 0 && !memberIds.Contains(u.IDUSER) && u.DISABLED == 0).ToList();
        }

        public void AddUserToGroup(decimal idGroup, decimal idMember)
        {
            try
            {
                var exists = db.TB_SYS_GROUP.Any(g => g.ID_GROUP == idGroup && g.MEMBER == idMember);
                if (!exists)
                {
                    var groupMember = new TB_SYS_GROUP { ID_GROUP = idGroup, MEMBER = idMember };
                    db.TB_SYS_GROUP.Add(groupMember);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi thêm thành viên vào nhóm: " + ex.Message);
            }
        }

        public void RemoveUserFromGroup(decimal idGroup, decimal idMember)
        {
            try
            {
                var record = db.TB_SYS_GROUP.FirstOrDefault(g => g.ID_GROUP == idGroup && g.MEMBER == idMember);
                if (record != null)
                {
                    db.TB_SYS_GROUP.Remove(record);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi xóa thành viên khỏi nhóm: " + ex.Message);
            }
        }

        public TB_SYS_USER Login(string username, string password)
        {
            string trimmedUsername = username.Trim().ToLower();
            
            // Prefer individual users (ISGROUP = 0), but fallback to any match (e.g. if ADMIN was saved with ISGROUP = 1)
            var user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == trimmedUsername && x.ISGROUP == 0);
            if (user == null)
            {
                user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == trimmedUsername);
            }

            if (user == null || user.DISABLED == 1) return null;

            if (PasswordHasher.VerifyPassword(password, user.PASSWORD))
            {
                return user;
            }
            return null;
        }

        public List<string> GetRights(decimal idUser)
        {
            var rights = new List<string>();

            // 1. Direct user rights
            var directRights = db.TB_SYS_RIGHT
                .Where(r => r.IDUSER == idUser && r.USER_RIGHT == 1)
                .Select(r => r.FUNCTION_CODE)
                .ToList();
            rights.AddRange(directRights);

            // 2. Group rights
            var groupIds = db.TB_SYS_GROUP
                .Where(g => g.MEMBER == idUser)
                .Select(g => g.ID_GROUP)
                .ToList();

            if (groupIds.Any())
            {
                var groupRights = db.TB_SYS_RIGHT
                    .Where(r => groupIds.Contains(r.IDUSER) && r.USER_RIGHT == 1)
                    .Select(r => r.FUNCTION_CODE)
                    .ToList();
                rights.AddRange(groupRights);
            }

            return rights.Distinct().ToList();
        }

        public void EnsureSeeded()
        {
            try
            {
                // Alter table PASSWORD column size to hold 60-character BCrypt hashes
                try
                {
                    db.Database.ExecuteSqlCommand("ALTER TABLE TB_SYS_USER MODIFY (PASSWORD NVARCHAR2(100))");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB MIGRATION]: Alter PASSWORD column skipped or failed (column might already be modified): {ex.Message}");
                }

                // Create or recreate TB_THONGBAO table if it does not exist or is outdated
                try
                {
                    int tableCount = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM user_tables WHERE table_name = 'TB_THONGBAO'"
                    ).FirstOrDefault();

                    bool needCreate = false;
                    if (tableCount > 0)
                    {
                        // Table exists, check if new column 'LOAI_TB' is present
                        int columnCount = db.Database.SqlQuery<int>(
                            "SELECT COUNT(*) FROM user_tab_cols WHERE table_name = 'TB_THONGBAO' AND column_name = 'LOAI_TB'"
                        ).FirstOrDefault();
                        if (columnCount == 0)
                        {
                            // Table is outdated (missing new columns), drop and recreate
                            db.Database.ExecuteSqlCommand("DROP TABLE TB_THONGBAO");
                            needCreate = true;
                        }
                    }
                    else
                    {
                        needCreate = true;
                    }

                    if (needCreate)
                    {
                        db.Database.ExecuteSqlCommand(@"
                            CREATE TABLE TB_THONGBAO (
                                ID NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                                TIEUDE NVARCHAR2(200) NOT NULL,
                                NOIDUNG NCLOB NOT NULL,
                                NGUOIDANG NVARCHAR2(50) NOT NULL,
                                NGAYDANG TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
                                LOAI_TB NVARCHAR2(100),
                                IS_PINNED NUMBER(1,0) DEFAULT 0,
                                TRANGTHAI NUMBER(1,0) DEFAULT 1,
                                NGAY_HETHAN TIMESTAMP,
                                FILE_DINHKEM NVARCHAR2(500),
                                MACTY NVARCHAR2(50),
                                MAPB NVARCHAR2(50)
                            )");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB MIGRATION]: Create/Recreate TB_THONGBAO table skipped or failed: {ex.Message}");
                }

                // Seed default announcement if empty
                try
                {
                    int announcementCount = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TB_THONGBAO").FirstOrDefault();
                    if (announcementCount == 0)
                    {
                        db.Database.ExecuteSqlCommand(@"
                            INSERT INTO TB_THONGBAO (TIEUDE, NOIDUNG, NGUOIDANG, NGAYDANG, LOAI_TB, IS_PINNED, TRANGTHAI, MACTY, MAPB)
                            VALUES ('Chào mừng', 'Chào mừng các bạn đến với phần mềm Quản lý nhân sự!', 'ADMIN', SYSTIMESTAMP, 'Thông báo chung', 1, 1, NULL, NULL)
                        ");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB MIGRATION]: Seed TB_THONGBAO table failed: {ex.Message}");
                }

                // 1. Seed Functions
                var functions = new List<TB_SYS_FUNCTION>
                {
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_GROUP", SORT = 1, DESCRIPTION = "Nhóm Người Dùng", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_USER", SORT = 2, DESCRIPTION = "Người Dùng", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_SAULUU", SORT = 3, DESCRIPTION = "Sao Lưu Dữ Liệu", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_PHUCHOI", SORT = 4, DESCRIPTION = "Phục Hồi Dữ Liệu", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_AI", SORT = 5, DESCRIPTION = "Trợ Lý AI", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_DANTOC", SORT = 10, DESCRIPTION = "Dân Tộc", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_TONGIAO", SORT = 11, DESCRIPTION = "Tôn Giáo", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_TRINHDO", SORT = 12, DESCRIPTION = "Trình Độ", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_NHANVIEN", SORT = 13, DESCRIPTION = "Nhân Viên", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_PHONGBAN", SORT = 14, DESCRIPTION = "Phòng Ban", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_BOPHAN", SORT = 15, DESCRIPTION = "Bộ Phận", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_CONGTY", SORT = 16, DESCRIPTION = "Công Ty", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_CHUCVU", SORT = 17, DESCRIPTION = "Chức Vụ", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_HOPDONG", SORT = 30, DESCRIPTION = "Hợp Đồng", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_NANGLUONG", SORT = 31, DESCRIPTION = "Lên Lương", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_KHENTHUONG", SORT = 32, DESCRIPTION = "Khen Thưởng", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_KYLUAT", SORT = 33, DESCRIPTION = "Kỷ Luật", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_DIEUCHUYEN", SORT = 34, DESCRIPTION = "Điều Chuyển", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_THOIVIEC", SORT = 35, DESCRIPTION = "Thôi Việc", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_LOAICA", SORT = 50, DESCRIPTION = "Loại Ca", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_LOAICONG", SORT = 51, DESCRIPTION = "Loại Công", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_PHUCAP", SORT = 52, DESCRIPTION = "Phụ Cấp", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_TANGCA", SORT = 53, DESCRIPTION = "Tăng Ca", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_UNGLUONG", SORT = 54, DESCRIPTION = "Ứng Lương", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BANGCONG", SORT = 55, DESCRIPTION = "Bảng Công", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BCCT", SORT = 56, DESCRIPTION = "Bảng Công Chi Tiết", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BANGLUONG", SORT = 57, DESCRIPTION = "Bảng Lương", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_BC_BAOCAO", SORT = 70, DESCRIPTION = "Báo Cáo Chi Tiết", ISGROUP = 0, MENU = 1, PARENT = "BC" }
                };

                foreach (var f in functions)
                {
                    if (!db.TB_SYS_FUNCTION.Any(x => x.FUNCTION_CODE == f.FUNCTION_CODE))
                    {
                        db.TB_SYS_FUNCTION.Add(f);
                    }
                }
                db.SaveChanges();

                // 2. Seed Users if table is empty
                if (!db.TB_SYS_USER.Any())
                {
                    var admin = new TB_SYS_USER { USERNAME = "ADMIN", PASSWORD = PasswordHasher.HashPassword("ADMIN"), FULLNAME = "ADMIN", ISGROUP = 1, DISABLED = 0, MACTY = "1", MADVI = "1" };
                    var nhansu = new TB_SYS_USER { USERNAME = "nhansu", PASSWORD = PasswordHasher.HashPassword("123"), FULLNAME = "HR Manager", ISGROUP = 0, DISABLED = 0, MACTY = "1", MADVI = "1" };
                    var chamcong = new TB_SYS_USER { USERNAME = "chamcong", PASSWORD = PasswordHasher.HashPassword("123"), FULLNAME = "Timekeeper", ISGROUP = 0, DISABLED = 0, MACTY = "1", MADVI = "1" };
                    var baocao = new TB_SYS_USER { USERNAME = "baocao", PASSWORD = PasswordHasher.HashPassword("123"), FULLNAME = "Report Viewer", ISGROUP = 0, DISABLED = 0, MACTY = "1", MADVI = "1" };

                    db.TB_SYS_USER.Add(admin);
                    db.TB_SYS_USER.Add(nhansu);
                    db.TB_SYS_USER.Add(chamcong);
                    db.TB_SYS_USER.Add(baocao);
                    db.SaveChanges();

                    // 3. Seed Rights
                    // nhansu: HR page (F_DM_..., F_NV_..., F_SYSTEM_AI)
                    var nhansuRights = new List<string> { "F_SYSTEM_AI", "F_DM_DANTOC", "F_DM_TONGIAO", "F_DM_TRINHDO", "F_DM_NHANVIEN", "F_DM_PHONGBAN", "F_DM_BOPHAN", "F_DM_CONGTY", "F_DM_CHUCVU", "F_NV_HOPDONG", "F_NV_NANGLUONG", "F_NV_KHENTHUONG", "F_NV_KYLUAT", "F_NV_DIEUCHUYEN", "F_NV_THOIVIEC" };
                    foreach (var code in nhansuRights)
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = nhansu.IDUSER, USER_RIGHT = 1 });
                    }

                    // chamcong: Timekeeping and Payroll (F_CC_..., F_SYSTEM_AI)
                    var chamcongRights = new List<string> { "F_SYSTEM_AI", "F_CC_LOAICA", "F_CC_LOAICONG", "F_CC_PHUCAP", "F_CC_TANGCA", "F_CC_UNGLUONG", "F_CC_BANGCONG", "F_CC_BCCT", "F_CC_BANGLUONG" };
                    foreach (var code in chamcongRights)
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = chamcong.IDUSER, USER_RIGHT = 1 });
                    }

                    // baocao: Reports only (F_BC_BAOCAO, F_SYSTEM_AI)
                    var baocaoRights = new List<string> { "F_SYSTEM_AI", "F_BC_BAOCAO" };
                    foreach (var code in baocaoRights)
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = baocao.IDUSER, USER_RIGHT = 1 });
                    }

                    db.SaveChanges();
                }

                // 4. Hash all existing plain text passwords in the database
                var allUsers = db.TB_SYS_USER.ToList();
                var unhashedUsers = allUsers
                    .Where(u => u.PASSWORD != null && u.PASSWORD.Trim() != "")
                    .Where(u => !u.PASSWORD.Trim().StartsWith("$2a$") && 
                                !u.PASSWORD.Trim().StartsWith("$2b$") && 
                                !u.PASSWORD.Trim().StartsWith("$2y$"))
                    .ToList();

                if (unhashedUsers.Any())
                {
                    foreach (var u in unhashedUsers)
                    {
                        string plainPassword = u.PASSWORD.Trim();
                        u.PASSWORD = PasswordHasher.HashPassword(plainPassword);
                    }
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine($"[MIGRATION]: Successfully hashed {unhashedUsers.Count} legacy plain text passwords in Oracle.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SEED ERROR]: {ex.Message}");
            }
        }
    }
}
