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
                    entity.MANV = user.MANV;
                    entity.CLIENT_TYPE = user.CLIENT_TYPE;
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
            
            // Prefer individual users (ISGROUP = 0), but fallback to any match
            var user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == trimmedUsername && x.ISGROUP == 0);
            if (user == null)
            {
                user = db.TB_SYS_USER.FirstOrDefault(x => x.USERNAME.Trim().ToLower() == trimmedUsername);
            }

            if (user == null) return null; // Username not found

            // Check if user is locked temporarily
            var lockoutInfo = db.Database.SqlQuery<DateTime?>("SELECT LOCKOUT_END FROM HR.TB_SYS_USER WHERE IDUSER = :id", 
                new Oracle.ManagedDataAccess.Client.OracleParameter("id", user.IDUSER)).FirstOrDefault();

            if (lockoutInfo.HasValue && lockoutInfo.Value > DateTime.Now)
            {
                var remaining = (int)(lockoutInfo.Value - DateTime.Now).TotalMinutes;
                if (remaining <= 0) remaining = 1;
                throw new ApplicationException($"TEMPORARY_LOCKED|{remaining}");
            }

            bool isPasswordValid = false;

            // 1. Quản trị viên hệ thống ADMIN: hỗ trợ các mật khẩu mặc định (admin, ADMIN, 123, 123456)
            if (trimmedUsername == "admin" && (password == "admin" || password == "ADMIN" || password == "123" || password == "123456"))
            {
                isPasswordValid = true;
            }
            else if (PasswordHasher.VerifyPassword(password, user.PASSWORD))
            {
                isPasswordValid = true;
            }
            else
            {
                string stored = (user.PASSWORD ?? "").Trim();
                if (!string.IsNullOrEmpty(stored) && stored.Equals(password, StringComparison.Ordinal))
                {
                    isPasswordValid = true;
                }
            }

            if (isPasswordValid)
            {
                if (user.DISABLED == 1)
                {
                    throw new ApplicationException("ACCOUNT_LOCKED");
                }

                // 2. Quy tắc phân định 2 loại tài khoản:
                // Tài khoản nhân viên (người dùng) chỉ dùng để đăng nhập Mobile, không được phép vào Desktop WinForms
                string clientType = (user.CLIENT_TYPE ?? "ALL").Trim().ToUpperInvariant();
                if (clientType == "MOBILE")
                {
                    throw new ApplicationException("EMPLOYEE_MOBILE_ONLY");
                }
                
                // Reset failed login count
                db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_USER SET FAILED_LOGIN_COUNT = 0, LOCKOUT_END = NULL WHERE IDUSER = :id", 
                    new Oracle.ManagedDataAccess.Client.OracleParameter("id", user.IDUSER));

                if (trimmedUsername == "admin")
                {
                    try
                    {
                        user.PASSWORD = PasswordHasher.HashPassword(password);
                        db.SaveChanges();
                    }
                    catch { }
                }
                    
                return user;
            }
            else
            {
                // Wrong password
                db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_USER SET FAILED_LOGIN_COUNT = NVL(FAILED_LOGIN_COUNT, 0) + 1 WHERE IDUSER = :id", 
                    new Oracle.ManagedDataAccess.Client.OracleParameter("id", user.IDUSER));
                    
                var failedCount = db.Database.SqlQuery<decimal>("SELECT NVL(FAILED_LOGIN_COUNT, 0) FROM HR.TB_SYS_USER WHERE IDUSER = :id", 
                    new Oracle.ManagedDataAccess.Client.OracleParameter("id", user.IDUSER)).FirstOrDefault();
                    
                if (failedCount >= 5)
                {
                    db.Database.ExecuteSqlCommand("UPDATE HR.TB_SYS_USER SET LOCKOUT_END = :lockTime WHERE IDUSER = :id", 
                        new Oracle.ManagedDataAccess.Client.OracleParameter("lockTime", DateTime.Now.AddMinutes(15)),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("id", user.IDUSER));
                    throw new ApplicationException("ACCOUNT_LOCKED_NOW");
                }
                
                throw new ApplicationException("WRONG_PASSWORD");
            }
        }

        public Dictionary<string, Bu.DTO.UserRightDetail> GetDetailedRights(decimal idUser)
        {
            var result = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase);

            // 1. Load all functions
            var allFuncs = db.TB_SYS_FUNCTION.OrderBy(f => f.SORT).ToList();
            foreach (var f in allFuncs)
            {
                result[f.FUNCTION_CODE] = new Bu.DTO.UserRightDetail
                {
                    FUNCTION_CODE = f.FUNCTION_CODE,
                    DESCRIPTION = f.DESCRIPTION,
                    CAN_VIEW = false,
                    CAN_ADD = false,
                    CAN_EDIT = false,
                    CAN_DELETE = false,
                    CAN_PRINT = false
                };
            }

            // 2. Direct user rights
            var directRights = db.TB_SYS_RIGHT
                .Where(r => r.IDUSER == idUser)
                .ToList();

            foreach (var r in directRights)
            {
                if (result.TryGetValue(r.FUNCTION_CODE, out var item))
                {
                    item.CAN_VIEW = (r.CAN_VIEW ?? 0) == 1 || (r.USER_RIGHT ?? 0) == 1;
                    item.CAN_ADD = (r.CAN_ADD ?? 0) == 1;
                    item.CAN_EDIT = (r.CAN_EDIT ?? 0) == 1;
                    item.CAN_DELETE = (r.CAN_DELETE ?? 0) == 1;
                    item.CAN_PRINT = (r.CAN_PRINT ?? 0) == 1;
                }
            }

            // 3. Group rights (inherit with logical OR)
            var groupIds = db.TB_SYS_GROUP
                .Where(g => g.MEMBER == idUser)
                .Select(g => g.ID_GROUP)
                .ToList();

            if (groupIds.Any())
            {
                var groupRights = db.TB_SYS_RIGHT
                    .Where(r => groupIds.Contains(r.IDUSER))
                    .ToList();

                foreach (var r in groupRights)
                {
                    if (result.TryGetValue(r.FUNCTION_CODE, out var item))
                    {
                        if ((r.CAN_VIEW ?? 0) == 1 || (r.USER_RIGHT ?? 0) == 1) item.CAN_VIEW = true;
                        if ((r.CAN_ADD ?? 0) == 1) item.CAN_ADD = true;
                        if ((r.CAN_EDIT ?? 0) == 1) item.CAN_EDIT = true;
                        if ((r.CAN_DELETE ?? 0) == 1) item.CAN_DELETE = true;
                        if ((r.CAN_PRINT ?? 0) == 1) item.CAN_PRINT = true;
                    }
                }
            }

            return result;
        }

        public List<string> GetRights(decimal idUser)
        {
            var rights = new List<string>();

            // 1. Direct user rights (support both CAN_VIEW and legacy USER_RIGHT)
            var directRights = db.TB_SYS_RIGHT
                .Where(r => r.IDUSER == idUser && ((r.CAN_VIEW.HasValue && r.CAN_VIEW.Value == 1) || (r.USER_RIGHT.HasValue && r.USER_RIGHT.Value == 1)))
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
                    .Where(r => groupIds.Contains(r.IDUSER) && ((r.CAN_VIEW.HasValue && r.CAN_VIEW.Value == 1) || (r.USER_RIGHT.HasValue && r.USER_RIGHT.Value == 1)))
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
                    System.Diagnostics.Debug.WriteLine($"[DB MIGRATION]: Alter PASSWORD column skipped or failed: {ex.Message}");
                }

                // Add MANV and CLIENT_TYPE to TB_SYS_USER if not present
                try
                {
                    int manvCol = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM user_tab_cols WHERE table_name = 'TB_SYS_USER' AND column_name = 'MANV'"
                    ).FirstOrDefault();
                    if (manvCol == 0)
                    {
                        db.Database.ExecuteSqlCommand("ALTER TABLE TB_SYS_USER ADD (MANV NUMBER)");
                    }

                    int clientTypeCol = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM user_tab_cols WHERE table_name = 'TB_SYS_USER' AND column_name = 'CLIENT_TYPE'"
                    ).FirstOrDefault();
                    if (clientTypeCol == 0)
                    {
                        db.Database.ExecuteSqlCommand("ALTER TABLE TB_SYS_USER ADD (CLIENT_TYPE NVARCHAR2(20) DEFAULT 'ALL')");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB MIGRATION]: Alter TB_SYS_USER MANV/CLIENT_TYPE skipped: {ex.Message}");
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
                    // === HỆ THỐNG & QUẢN TRỊ ===
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_GROUP", SORT = 1, DESCRIPTION = "Nhóm Người Dùng", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_USER", SORT = 2, DESCRIPTION = "Quản Lý Tài Khoản", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_CAPTAIKHOAN", SORT = 3, DESCRIPTION = "Cấp Tài Khoản Hàng Loạt", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_LOCK_USER", SORT = 4, DESCRIPTION = "Khóa/Mở Khóa Tài Khoản", ISGROUP = 0, MENU = 0, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_PQ_CHUCNANG", SORT = 5, DESCRIPTION = "Phân Quyền Chức Năng", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_PQ_BAOCAO", SORT = 6, DESCRIPTION = "Phân Quyền Báo Cáo", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_THONGBAO", SORT = 7, DESCRIPTION = "Thông Báo Hệ Thống", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_SAULUU", SORT = 8, DESCRIPTION = "Sao Lưu Dữ Liệu", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_PHUCHOI", SORT = 9, DESCRIPTION = "Phục Hồi Dữ Liệu", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_GIAMSAT", SORT = 10, DESCRIPTION = "Giám Sát Đăng Nhập", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_AI", SORT = 11, DESCRIPTION = "Trợ Lý AI & Chatbot", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_SETTING", SORT = 12, DESCRIPTION = "Cấu Hình Ngôn Ngữ & Hệ Thống", ISGROUP = 0, MENU = 1, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_AI_CONFIG", SORT = 13, DESCRIPTION = "Cấu Hình AI Server (Ollama)", ISGROUP = 0, MENU = 0, PARENT = "SYSTEM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_SYSTEM_DB_CONFIG", SORT = 14, DESCRIPTION = "Cấu Hình Kết Nối CSDL", ISGROUP = 0, MENU = 0, PARENT = "SYSTEM" },

                    // === DASHBOARD & THỐNG KÊ ===
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DB_NHANSU", SORT = 20, DESCRIPTION = "Dashboard Nhân Sự", ISGROUP = 0, MENU = 1, PARENT = "DASHBOARD" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DB_LUONG", SORT = 21, DESCRIPTION = "Dashboard Lương", ISGROUP = 0, MENU = 1, PARENT = "DASHBOARD" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_BC_BAOCAO", SORT = 22, DESCRIPTION = "Báo Cáo Tổng Hợp & Chi Tiết", ISGROUP = 0, MENU = 1, PARENT = "DASHBOARD" },

                    // === DANH MỤC DÙNG CHUNG ===
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_DANTOC", SORT = 30, DESCRIPTION = "Dân Tộc", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_TONGIAO", SORT = 31, DESCRIPTION = "Tôn Giáo", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_TRINHDO", SORT = 32, DESCRIPTION = "Trình Độ", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_NHANVIEN", SORT = 33, DESCRIPTION = "Hồ Sơ Nhân Viên", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_PHONGBAN", SORT = 34, DESCRIPTION = "Phòng Ban", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_BOPHAN", SORT = 35, DESCRIPTION = "Bộ Phận", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_CONGTY", SORT = 36, DESCRIPTION = "Công Ty", ISGROUP = 0, MENU = 1, PARENT = "DM" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_DM_CHUCVU", SORT = 37, DESCRIPTION = "Chức Vụ", ISGROUP = 0, MENU = 1, PARENT = "DM" },

                    // === QUẢN LÝ NHÂN SỰ & NGHIỆP VỤ ===
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_HOPDONG", SORT = 50, DESCRIPTION = "Hợp Đồng Lao Động", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_LOAIHOPDONG", SORT = 51, DESCRIPTION = "Loại Hợp Đồng", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_NANGLUONG", SORT = 52, DESCRIPTION = "Lên Lương Nhân Viên", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_KHENTHUONG", SORT = 53, DESCRIPTION = "Khen Thưởng", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_KYLUAT", SORT = 54, DESCRIPTION = "Kỷ Luật", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_DIEUCHUYEN", SORT = 55, DESCRIPTION = "Điều Chuyển Nhân Viên", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_THOIVIEC", SORT = 56, DESCRIPTION = "Thôi Việc", ISGROUP = 0, MENU = 1, PARENT = "NV" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_NV_PHEDUYET", SORT = 57, DESCRIPTION = "Phê Duyệt Yêu Cầu (Online)", ISGROUP = 0, MENU = 1, PARENT = "NV" },

                    // === CHẤM CÔNG & TIỀN LƯƠNG ===
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_LOAICA", SORT = 70, DESCRIPTION = "Loại Ca", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_LOAICONG", SORT = 71, DESCRIPTION = "Loại Công", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_NGAYLE", SORT = 72, DESCRIPTION = "Ngày Lễ", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_PHUCAP", SORT = 73, DESCRIPTION = "Phụ Cấp Nhân Viên", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_TANGCA", SORT = 74, DESCRIPTION = "Tăng Ca", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_UNGLUONG", SORT = 75, DESCRIPTION = "Ứng Lương", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BANGCONG", SORT = 76, DESCRIPTION = "Quản Lý Bảng Công", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BCCT", SORT = 77, DESCRIPTION = "Bảng Công Chi Tiết", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BCCT_IN", SORT = 78, DESCRIPTION = "In Bảng Công Nhân Viên", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_CAPNHATCONG", SORT = 79, DESCRIPTION = "Cập Nhật Ngày Công", ISGROUP = 0, MENU = 1, PARENT = "CC" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "F_CC_BANGLUONG", SORT = 80, DESCRIPTION = "Bảng Lương", ISGROUP = 0, MENU = 1, PARENT = "CC" },

                    // === DỊCH VỤ MOBILE APP ===
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_ROOT", SORT = 200, DESCRIPTION = "Phân Hệ Mobile App", ISGROUP = 0, MENU = 1, PARENT = "MOBILE" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_PROFILE_VIEW", SORT = 201, DESCRIPTION = "Xem Hồ Sơ Cá Nhân Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_ATTENDANCE_VIEW", SORT = 202, DESCRIPTION = "Xem Bảng Công Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_PAYROLL_VIEW", SORT = 203, DESCRIPTION = "Xem Bảng Lương Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_CONTRACT_VIEW", SORT = 204, DESCRIPTION = "Xem Hợp Đồng Lao Động Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_INSURANCE_VIEW", SORT = 205, DESCRIPTION = "Xem Bảo Hiểm Xã Hội Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_NOTIFICATION_VIEW", SORT = 206, DESCRIPTION = "Xem Thông Báo Nội Bộ Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_REQUEST_LEAVE", SORT = 207, DESCRIPTION = "Gửi Đơn Nghỉ Phép Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_REQUEST_OVERTIME", SORT = 208, DESCRIPTION = "Gửi Đơn Tăng Ca Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" },
                    new TB_SYS_FUNCTION { FUNCTION_CODE = "MOBILE_REQUEST_ADVANCE", SORT = 209, DESCRIPTION = "Gửi Yêu Cầu Ứng Lương Mobile", ISGROUP = 0, MENU = 1, PARENT = "MOBILE_ROOT" }
                };

                foreach (var f in functions)
                {
                    try
                    {
                        var existing = db.TB_SYS_FUNCTION.FirstOrDefault(x => x.FUNCTION_CODE == f.FUNCTION_CODE);
                        if (existing == null)
                        {
                            db.TB_SYS_FUNCTION.Add(new TB_SYS_FUNCTION
                            {
                                FUNCTION_CODE = f.FUNCTION_CODE,
                                SORT = f.SORT,
                                DESCRIPTION = f.DESCRIPTION,
                                PARENT = f.PARENT,
                                MENU = f.MENU,
                                ISGROUP = f.ISGROUP
                            });
                        }
                        else
                        {
                            // In EF6, SORT is part of composite EntityKey, so do not modify existing.SORT directly on entity
                            existing.DESCRIPTION = f.DESCRIPTION;
                            existing.PARENT = f.PARENT;
                            existing.MENU = f.MENU;
                            existing.ISGROUP = f.ISGROUP;
                        }
                        db.SaveChanges();
                    }
                    catch (Exception exFunc)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DB MIGRATION]: Seed function {f.FUNCTION_CODE} error: {exFunc.Message}");
                    }
                }

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
                    // nhansu: HR page
                    var nhansuRights = new List<string> { 
                        "F_SYSTEM_AI", "F_DM_DANTOC", "F_DM_TONGIAO", "F_DM_TRINHDO", "F_DM_NHANVIEN", 
                        "F_DM_PHONGBAN", "F_DM_BOPHAN", "F_DM_CONGTY", "F_DM_CHUCVU", "F_NV_HOPDONG", 
                        "F_NV_LOAIHOPDONG", "F_NV_NANGLUONG", "F_NV_KHENTHUONG", "F_NV_KYLUAT", 
                        "F_NV_DIEUCHUYEN", "F_NV_THOIVIEC", "F_NV_PHEDUYET", "F_SYSTEM_THONGBAO" 
                    };
                    foreach (var code in nhansuRights)
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = nhansu.IDUSER, USER_RIGHT = 1, CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1 });
                    }

                    // chamcong: Timekeeping and Payroll
                    var chamcongRights = new List<string> { 
                        "F_SYSTEM_AI", "F_CC_LOAICA", "F_CC_LOAICONG", "F_CC_NGAYLE", "F_CC_PHUCAP", 
                        "F_CC_TANGCA", "F_CC_UNGLUONG", "F_CC_BANGCONG", "F_CC_BCCT", "F_CC_BCCT_IN", 
                        "F_CC_CAPNHATCONG", "F_CC_BANGLUONG" 
                    };
                    foreach (var code in chamcongRights)
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = chamcong.IDUSER, USER_RIGHT = 1, CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1 });
                    }

                    // baocao: Reports only
                    var baocaoRights = new List<string> { "F_SYSTEM_AI", "F_BC_BAOCAO", "F_DB_NHANSU", "F_DB_LUONG" };
                    foreach (var code in baocaoRights)
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = baocao.IDUSER, USER_RIGHT = 1, CAN_VIEW = 1, CAN_PRINT = 1 });
                    }

                    db.SaveChanges();
                }
                else
                {
                    // Update default users with new rights if missing
                    var nhansuUser = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME.ToLower() == "nhansu");
                    if (nhansuUser != null)
                    {
                        var extraRights = new List<string> { "F_NV_PHEDUYET", "F_SYSTEM_THONGBAO", "F_NV_LOAIHOPDONG" };
                        foreach (var code in extraRights)
                        {
                            if (!db.TB_SYS_RIGHT.Any(r => r.IDUSER == nhansuUser.IDUSER && r.FUNCTION_CODE == code))
                            {
                                db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = nhansuUser.IDUSER, USER_RIGHT = 1, CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1 });
                            }
                        }
                    }

                    var chamcongUser = db.TB_SYS_USER.FirstOrDefault(u => u.USERNAME.ToLower() == "chamcong");
                    if (chamcongUser != null)
                    {
                        var extraRights = new List<string> { "F_CC_NGAYLE", "F_CC_BCCT_IN", "F_CC_CAPNHATCONG" };
                        foreach (var code in extraRights)
                        {
                            if (!db.TB_SYS_RIGHT.Any(r => r.IDUSER == chamcongUser.IDUSER && r.FUNCTION_CODE == code))
                            {
                                db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT { FUNCTION_CODE = code, IDUSER = chamcongUser.IDUSER, USER_RIGHT = 1, CAN_VIEW = 1, CAN_ADD = 1, CAN_EDIT = 1, CAN_DELETE = 1, CAN_PRINT = 1 });
                            }
                        }
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

                // 5. Seed Translations for AI Form
                var newTranslations = new List<TB_TRANSLATIONS>
                {
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu hình AI", LANGUAGE_CODE = "EN", VALUE = "AI Config" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu hình AI", LANGUAGE_CODE = "JA", VALUE = "AI設定" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu hình AI", LANGUAGE_CODE = "ZH", VALUE = "AI配置" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu hình AI", LANGUAGE_CODE = "KO", VALUE = "AI 구성" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối AI (Ollama)", LANGUAGE_CODE = "EN", VALUE = "AI Connection (Ollama)" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối AI (Ollama)", LANGUAGE_CODE = "JA", VALUE = "AI接続 (Ollama)" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối AI (Ollama)", LANGUAGE_CODE = "ZH", VALUE = "AI连接 (Ollama)" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối AI (Ollama)", LANGUAGE_CODE = "KO", VALUE = "AI 연결 (Ollama)" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Kiểm Tra Kết Nối", LANGUAGE_CODE = "EN", VALUE = "Test Connection" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Kiểm Tra Kết Nối", LANGUAGE_CODE = "JA", VALUE = "接続テスト" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Kiểm Tra Kết Nối", LANGUAGE_CODE = "ZH", VALUE = "测试连接" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Kiểm Tra Kết Nối", LANGUAGE_CODE = "KO", VALUE = "연결 테스트" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Lưu Thiết Lập", LANGUAGE_CODE = "EN", VALUE = "Save Settings" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Lưu Thiết Lập", LANGUAGE_CODE = "JA", VALUE = "設定を保存" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Lưu Thiết Lập", LANGUAGE_CODE = "ZH", VALUE = "保存设置" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Lưu Thiết Lập", LANGUAGE_CODE = "KO", VALUE = "설정 저장" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Đang thử...", LANGUAGE_CODE = "EN", VALUE = "Testing..." },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Đang thử...", LANGUAGE_CODE = "JA", VALUE = "テスト中..." },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Đang thử...", LANGUAGE_CODE = "ZH", VALUE = "测试中..." },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Đang thử...", LANGUAGE_CODE = "KO", VALUE = "테스트 중..." },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình AI Server", LANGUAGE_CODE = "EN", VALUE = "AI Server Config" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình AI Server", LANGUAGE_CODE = "JA", VALUE = "AIサーバー設定" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình AI Server", LANGUAGE_CODE = "ZH", VALUE = "AI服务器配置" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình AI Server", LANGUAGE_CODE = "KO", VALUE = "AI 서버 구성" },

                    // Core UI Translations
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phân hệ", LANGUAGE_CODE = "EN", VALUE = "Subsystem" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phân hệ", LANGUAGE_CODE = "JA", VALUE = "サブシステム" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phân hệ", LANGUAGE_CODE = "ZH", VALUE = "子系统" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phân hệ", LANGUAGE_CODE = "KO", VALUE = "하위 시스템" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phê Duyệt Yêu Cầu (Online)", LANGUAGE_CODE = "EN", VALUE = "Online Request Approvals" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phê Duyệt Yêu Cầu (Online)", LANGUAGE_CODE = "JA", VALUE = "オンライン申請承認" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phê Duyệt Yêu Cầu (Online)", LANGUAGE_CODE = "ZH", VALUE = "在线申请审批" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Phê Duyệt Yêu Cầu (Online)", LANGUAGE_CODE = "KO", VALUE = "온라인 신청 결재" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Thông Báo Hệ Thống", LANGUAGE_CODE = "EN", VALUE = "System Announcements" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Thông Báo Hệ Thống", LANGUAGE_CODE = "JA", VALUE = "システム通知" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Thông Báo Hệ Thống", LANGUAGE_CODE = "ZH", VALUE = "系统公告" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Thông Báo Hệ Thống", LANGUAGE_CODE = "KO", VALUE = "시스템 공지" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấp Tài Khoản Hàng Loạt", LANGUAGE_CODE = "EN", VALUE = "Bulk Account Provisioning" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấp Tài Khoản Hàng Loạt", LANGUAGE_CODE = "JA", VALUE = "一括アカウント発行" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấp Tài Khoản Hàng Loạt", LANGUAGE_CODE = "ZH", VALUE = "批量账号分配" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấp Tài Khoản Hàng Loạt", LANGUAGE_CODE = "KO", VALUE = "일괄 계정 발급" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối CSDL", LANGUAGE_CODE = "EN", VALUE = "Database Connection Config" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối CSDL", LANGUAGE_CODE = "JA", VALUE = "DB接続設定" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối CSDL", LANGUAGE_CODE = "ZH", VALUE = "数据库连接配置" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Cấu Hình Kết Nối CSDL", LANGUAGE_CODE = "KO", VALUE = "데이터베이스 연결 설정" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Chọn tất cả", LANGUAGE_CODE = "EN", VALUE = "Select All" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Chọn tất cả", LANGUAGE_CODE = "JA", VALUE = "すべて選択" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Chọn tất cả", LANGUAGE_CODE = "ZH", VALUE = "全选" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Chọn tất cả", LANGUAGE_CODE = "KO", VALUE = "모두 선택" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Bỏ tất cả", LANGUAGE_CODE = "EN", VALUE = "Deselect All" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Bỏ tất cả", LANGUAGE_CODE = "JA", VALUE = "すべて解除" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Bỏ tất cả", LANGUAGE_CODE = "ZH", VALUE = "取消全选" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Bỏ tất cả", LANGUAGE_CODE = "KO", VALUE = "모두 해제" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Sửa quyền", LANGUAGE_CODE = "EN", VALUE = "Edit Rights" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Sửa quyền", LANGUAGE_CODE = "JA", VALUE = "権限編集" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Sửa quyền", LANGUAGE_CODE = "ZH", VALUE = "编辑权限" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Sửa quyền", LANGUAGE_CODE = "KO", VALUE = "권한 편집" },

                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Làm mới", LANGUAGE_CODE = "EN", VALUE = "Refresh" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Làm mới", LANGUAGE_CODE = "JA", VALUE = "更新" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Làm mới", LANGUAGE_CODE = "ZH", VALUE = "刷新" },
                    new TB_TRANSLATIONS { TABLE_NAME = "UI_LABEL", COLUMN_NAME = "Làm mới", LANGUAGE_CODE = "KO", VALUE = "새로고침" },
                };

                foreach (var trans in newTranslations)
                {
                    if (!db.TB_TRANSLATIONS.Any(t => t.TABLE_NAME == trans.TABLE_NAME && t.COLUMN_NAME == trans.COLUMN_NAME && t.LANGUAGE_CODE == trans.LANGUAGE_CODE))
                    {
                        trans.RECORD_ID = Guid.NewGuid().ToString();
                        db.TB_TRANSLATIONS.Add(trans);
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SEED ERROR]: {ex.Message}");
            }
        }
        public List<Bu.DTO.USER_LOGIN_INFO_DTO> GetDashboardUserLoginInfo()
        {
            using (var db = new MyEntities())
            {
                string sql = @"
                    SELECT 
                        u.IDUSER, u.USERNAME, u.FULLNAME, 
                        CASE WHEN NVL(u.DISABLED, 0) = 1 THEN 'Bị khóa' ELSE 'Đang hoạt động' END as TRANGTHAI_HOATDONG,
                        lh.IP_ADDRESS, lh.MAC_ADDRESS, lh.TEN_MAY_TINH, 
                        lh.THOIGIAN AS THOIGIAN_DANGNHAP, lh.THOIGIAN_DANGXUAT, lh.TRANGTHAI AS TRANGTHAI_DANGNHAP,
                        CASE 
                            WHEN lh.THOIGIAN IS NOT NULL AND lh.THOIGIAN_DANGXUAT IS NULL AND lh.TRANGTHAI = 'Thành công' THEN 'Đang Online'
                            ELSE 'Offline'
                        END AS TRANGTHAI_ONLINE
                    FROM HR.TB_SYS_USER u
                    LEFT JOIN (
                        SELECT ID_USER, IP_ADDRESS, MAC_ADDRESS, TEN_MAY_TINH, THOIGIAN, THOIGIAN_DANGXUAT, TRANGTHAI,
                               ROW_NUMBER() OVER(PARTITION BY ID_USER ORDER BY THOIGIAN DESC) as rn
                        FROM HR.TB_SYS_LOGIN_HISTORY
                    ) lh ON u.IDUSER = lh.ID_USER AND lh.rn = 1
                    ORDER BY u.IDUSER ASC";
                
                return db.Database.SqlQuery<Bu.DTO.USER_LOGIN_INFO_DTO>(sql).ToList();
            }
        }
    }
}
