using Bu.CLASS_SYSTEM;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace HRMS_API.Controllers
{
    [RoutePrefix("api/users")]
    public class UserController : ApiController
    {
        private readonly SYS_USER _userBus = new SYS_USER();

        /// <summary>
        /// GET: api/users
        /// Lấy danh sách tài khoản người dùng và nhóm quyền với đầy đủ thông tin chuẩn hóa
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAllUsers()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var rawUsers = db.TB_SYS_USER.ToList();
                    var allGroups = db.TB_SYS_GROUP.ToList();

                    var list = rawUsers.Select(u =>
                    {
                        bool isGroup = (u.ISGROUP ?? 0) == 1;
                        bool isDisabled = (u.DISABLED ?? 0) == 1;
                        bool isAdmin = u.USERNAME != null && u.USERNAME.Trim().ToUpper() == "ADMIN";

                        int memberCount = isGroup ? allGroups.Count(g => g.ID_GROUP == u.IDUSER) : 0;
                        
                        List<string> groupNames = new List<string>();
                        if (!isGroup)
                        {
                            var parentGroupIds = allGroups.Where(g => g.MEMBER == u.IDUSER).Select(g => g.ID_GROUP).ToList();
                            groupNames = rawUsers.Where(grp => parentGroupIds.Contains(grp.IDUSER))
                                                 .Select(grp => grp.FULLNAME ?? grp.USERNAME)
                                                 .ToList();
                        }

                        return new
                        {
                            IdUser = (int)u.IDUSER,
                            id = (int)u.IDUSER,
                            IDUSER = (int)u.IDUSER,
                            Username = u.USERNAME,
                            username = u.USERNAME,
                            USERNAME = u.USERNAME,
                            FullName = u.FULLNAME ?? u.USERNAME,
                            fullName = u.FULLNAME ?? u.USERNAME,
                            FULLNAME = u.FULLNAME ?? u.USERNAME,
                            IsGroup = isGroup,
                            isGroup = isGroup,
                            ISGROUP = isGroup ? 1 : 0,
                            Disabled = isDisabled,
                            disabled = isDisabled,
                            DISABLED = isDisabled ? 1 : 0,
                            IsAdmin = isAdmin,
                            isAdmin = isAdmin,
                            MemberCount = memberCount,
                            memberCount = memberCount,
                            Groups = groupNames,
                            groups = groupNames,
                            MACTY = u.MACTY,
                            MADVI = u.MADVI
                        };
                    }).OrderBy(u => u.IsGroup ? 0 : 1).ThenBy(u => u.Username).ToList();

                    return Ok(list);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh sách người dùng: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/users
        /// Tạo mới tài khoản người dùng hoặc nhóm quyền
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateUser([FromBody] CreateUserRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.Username))
                {
                    return BadRequest("Tên đăng nhập / Mã nhóm không được để trống.");
                }

                using (var db = new MyEntities())
                {
                    string cleanUsername = req.Username.Trim();
                    if (db.TB_SYS_USER.Any(u => u.USERNAME.Trim().ToLower() == cleanUsername.ToLower()))
                    {
                        return BadRequest($"Tên tài khoản / Mã nhóm '{cleanUsername}' đã tồn tại trong hệ thống.");
                    }

                    var newUser = new TB_SYS_USER
                    {
                        USERNAME = cleanUsername,
                        FULLNAME = req.FullName ?? cleanUsername,
                        PASSWORD = req.IsGroup ? "" : PasswordHasher.HashPassword(string.IsNullOrWhiteSpace(req.Password) ? "123" : req.Password),
                        ISGROUP = req.IsGroup ? 1 : 0,
                        DISABLED = 0,
                        MACTY = "1",
                        MADVI = "1"
                    };

                    db.TB_SYS_USER.Add(newUser);
                    db.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        IdUser = (int)newUser.IDUSER,
                        id = (int)newUser.IDUSER,
                        Username = newUser.USERNAME,
                        FullName = newUser.FULLNAME,
                        IsGroup = req.IsGroup,
                        message = req.IsGroup ? $"Đã tạo nhóm quyền [{newUser.USERNAME}] thành công." : $"Đã tạo tài khoản [{newUser.USERNAME}] thành công."
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tạo tài khoản / nhóm: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// PUT: api/users/{id}
        /// Cập nhật thông tin tài khoản hoặc nhóm quyền
        /// </summary>
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult UpdateUser(int id, [FromBody] UpdateUserRequest req)
        {
            try
            {
                if (req == null) return BadRequest("Dữ liệu không hợp lệ.");

                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (!string.IsNullOrWhiteSpace(req.FullName)) user.FULLNAME = req.FullName;
                    
                    if (req.Disabled.HasValue)
                    {
                        if (user.USERNAME.Trim().ToUpper() == "ADMIN" && req.Disabled.Value)
                        {
                            return BadRequest("Không thể khóa tài khoản Quản trị hệ thống (ADMIN).");
                        }
                        user.DISABLED = req.Disabled.Value ? 1 : 0;
                    }

                    if (!string.IsNullOrWhiteSpace(req.NewPassword))
                    {
                        user.PASSWORD = PasswordHasher.HashPassword(req.NewPassword);
                    }

                    db.SaveChanges();
                    return Ok(new { success = true, message = $"Đã cập nhật thông tin tài khoản #{id} ({user.USERNAME})." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi cập nhật tài khoản: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// PUT: api/users/{id}/toggle-lock
        /// Khóa hoặc mở khóa nhanh tài khoản người dùng
        /// </summary>
        [HttpPut]
        [Route("{id:int}/toggle-lock")]
        public IHttpActionResult ToggleLock(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (user.USERNAME.Trim().ToUpper() == "ADMIN")
                    {
                        return BadRequest("Không thể khóa tài khoản Quản trị hệ thống (ADMIN).");
                    }

                    bool willLock = (user.DISABLED ?? 0) == 0;
                    user.DISABLED = willLock ? 1 : 0;
                    db.SaveChanges();

                    return Ok(new
                    {
                        success = true,
                        disabled = willLock,
                        message = willLock ? $"Đã khóa tài khoản [{user.USERNAME}]." : $"Đã mở khóa tài khoản [{user.USERNAME}]."
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi khóa/mở khóa tài khoản: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/users/{id}/reset-password
        /// Quản trị viên đặt lại mật khẩu cho tài khoản
        /// </summary>
        [HttpPost]
        [Route("{id:int}/reset-password")]
        public IHttpActionResult ResetPassword(int id, [FromBody] ResetPasswordRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.NewPassword))
                {
                    return BadRequest("Vui lòng nhập mật khẩu mới.");
                }

                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    user.PASSWORD = PasswordHasher.HashPassword(req.NewPassword);
                    db.SaveChanges();

                    return Ok(new { success = true, message = $"Đã đặt lại mật khẩu cho tài khoản [{user.USERNAME}] thành công." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi đặt lại mật khẩu: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// DELETE: api/users/{id}
        /// Xóa tài khoản hoặc nhóm quyền, đồng thời dọn dẹp quan hệ TB_SYS_GROUP và TB_SYS_RIGHT
        /// </summary>
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult DeleteUser(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var user = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == id);
                    if (user == null) return NotFound();

                    if (user.USERNAME.Trim().ToUpper() == "ADMIN")
                    {
                        return BadRequest("Không thể xóa tài khoản Quản trị hệ thống (ADMIN).");
                    }

                    // 1. Xóa phân quyền trong TB_SYS_RIGHT
                    var rights = db.TB_SYS_RIGHT.Where(r => r.IDUSER == id).ToList();
                    db.TB_SYS_RIGHT.RemoveRange(rights);

                    // 2. Xóa liên kết nhóm trong TB_SYS_GROUP
                    var groupMemberships = db.TB_SYS_GROUP.Where(g => g.MEMBER == id || g.ID_GROUP == id).ToList();
                    db.TB_SYS_GROUP.RemoveRange(groupMemberships);

                    // 3. Xóa phân quyền báo cáo nếu có
                    var reportRights = db.TB_SYS_RIGHT_REPORT.Where(r => r.IDUSER == id).ToList();
                    db.TB_SYS_RIGHT_REPORT.RemoveRange(reportRights);

                    // 4. Xóa đối tượng người dùng / nhóm
                    db.TB_SYS_USER.Remove(user);
                    db.SaveChanges();

                    return Ok(new { success = true, message = $"Đã xóa tài khoản / nhóm [{user.USERNAME}] thành công." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi xóa người dùng/nhóm: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/users/groups/{groupId}/members
        /// Lấy danh sách thành viên hiện tại của nhóm và danh sách người dùng chưa thuộc nhóm
        /// </summary>
        [HttpGet]
        [Route("groups/{groupId:int}/members")]
        public IHttpActionResult GetGroupMembers(int groupId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var group = db.TB_SYS_USER.FirstOrDefault(u => u.IDUSER == groupId && (u.ISGROUP ?? 0) == 1);
                    if (group == null) return NotFound();

                    var memberIds = db.TB_SYS_GROUP.Where(g => g.ID_GROUP == groupId).Select(g => g.MEMBER).ToList();
                    var allUsers = db.TB_SYS_USER.Where(u => (u.ISGROUP ?? 0) == 0).ToList();

                    var members = allUsers.Where(u => memberIds.Contains(u.IDUSER)).Select(u => new
                    {
                        IdUser = (int)u.IDUSER,
                        id = (int)u.IDUSER,
                        Username = u.USERNAME,
                        username = u.USERNAME,
                        FullName = u.FULLNAME ?? u.USERNAME,
                        fullName = u.FULLNAME ?? u.USERNAME,
                        Disabled = (u.DISABLED ?? 0) == 1
                    }).ToList();

                    var availableUsers = allUsers.Where(u => !memberIds.Contains(u.IDUSER) && (u.DISABLED ?? 0) == 0).Select(u => new
                    {
                        IdUser = (int)u.IDUSER,
                        id = (int)u.IDUSER,
                        Username = u.USERNAME,
                        username = u.USERNAME,
                        FullName = u.FULLNAME ?? u.USERNAME,
                        fullName = u.FULLNAME ?? u.USERNAME
                    }).ToList();

                    return Ok(new
                    {
                        GroupId = groupId,
                        GroupName = group.USERNAME,
                        GroupFullName = group.FULLNAME,
                        Members = members,
                        members = members,
                        AvailableUsers = availableUsers,
                        availableUsers = availableUsers
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải thành viên nhóm: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/users/groups/{groupId}/members/{memberId}
        /// Thêm người dùng vào nhóm quyền (TB_SYS_GROUP)
        /// </summary>
        [HttpPost]
        [Route("groups/{groupId:int}/members/{memberId:int}")]
        public IHttpActionResult AddMemberToGroup(int groupId, int memberId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    bool exists = db.TB_SYS_GROUP.Any(g => g.ID_GROUP == groupId && g.MEMBER == memberId);
                    if (!exists)
                    {
                        db.TB_SYS_GROUP.Add(new TB_SYS_GROUP
                        {
                            ID_GROUP = groupId,
                            MEMBER = memberId
                        });
                        db.SaveChanges();
                    }

                    return Ok(new { success = true, message = "Đã thêm thành viên vào nhóm thành công." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi thêm thành viên vào nhóm: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// DELETE: api/users/groups/{groupId}/members/{memberId}
        /// Gỡ người dùng khỏi nhóm quyền (TB_SYS_GROUP)
        /// </summary>
        [HttpDelete]
        [Route("groups/{groupId:int}/members/{memberId:int}")]
        public IHttpActionResult RemoveMemberFromGroup(int groupId, int memberId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    var record = db.TB_SYS_GROUP.FirstOrDefault(g => g.ID_GROUP == groupId && g.MEMBER == memberId);
                    if (record != null)
                    {
                        db.TB_SYS_GROUP.Remove(record);
                        db.SaveChanges();
                    }

                    return Ok(new { success = true, message = "Đã gỡ thành viên khỏi nhóm thành công." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi gỡ thành viên khỏi nhóm: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/users/{id}/rights
        /// Lấy toàn bộ danh sách chức năng hệ thống kèm cờ HAS_RIGHT cho user/group tương ứng
        /// </summary>
        [HttpGet]
        [Route("{id:int}/rights")]
        public IHttpActionResult GetUserRights(int id)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var allFunctions = db.TB_SYS_FUNCTION.OrderBy(f => f.SORT).ToList();

                    // Lấy quyền gán trực tiếp
                    var directRights = db.TB_SYS_RIGHT
                        .Where(r => r.IDUSER == id && r.USER_RIGHT == 1)
                        .Select(r => r.FUNCTION_CODE)
                        .ToList();

                    var result = allFunctions.Select(f => new
                    {
                        FUNCTION_CODE = f.FUNCTION_CODE,
                        FuncCode = f.FUNCTION_CODE,
                        funcCode = f.FUNCTION_CODE,
                        DESCRIPTION = f.DESCRIPTION,
                        Description = f.DESCRIPTION,
                        description = f.DESCRIPTION,
                        PARENT = f.PARENT,
                        Parent = f.PARENT,
                        parent = f.PARENT,
                        SORT = f.SORT,
                        HAS_RIGHT = directRights.Contains(f.FUNCTION_CODE),
                        HasRight = directRights.Contains(f.FUNCTION_CODE),
                        hasRight = directRights.Contains(f.FUNCTION_CODE)
                    }).ToList();

                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi lấy quyền người dùng: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// POST: api/users/{id}/rights
        /// Cập nhật quyền chức năng cho user/group (Tương ứng FrmPhanQuyenChucNang trong WinForms)
        /// </summary>
        [HttpPost]
        [Route("{id:int}/rights")]
        public IHttpActionResult SaveUserRights(int id, [FromBody] SaveRightsRequest req)
        {
            try
            {
                var codes = req != null ? req.GetEffectiveCodes() : new List<string>();
                if (codes == null)
                {
                    return BadRequest("Danh sách quyền không hợp lệ.");
                }

                using (var db = new MyEntities())
                {
                    // Lấy tất cả các quyền hiện có của user
                    var existing = db.TB_SYS_RIGHT.Where(r => r.IDUSER == id).ToList();
                    db.TB_SYS_RIGHT.RemoveRange(existing);
                    db.SaveChanges();

                    // Thêm các quyền mới
                    foreach (var code in codes.Distinct())
                    {
                        db.TB_SYS_RIGHT.Add(new TB_SYS_RIGHT
                        {
                            IDUSER = id,
                            FUNCTION_CODE = code,
                            USER_RIGHT = 1
                        });
                    }
                    db.SaveChanges();

                    return Ok(new { success = true, message = $"Đã cập nhật phân quyền thành công cho tài khoản/nhóm #{id} ({codes.Count} quyền)." });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi lưu phân quyền: " + ex.Message, ex));
            }
        }

        /// <summary>
        /// GET: api/users/functions
        /// Danh mục toàn bộ chức năng hệ thống
        /// </summary>
        [HttpGet]
        [Route("functions")]
        public IHttpActionResult GetFunctions()
        {
            try
            {
                using (var db = new MyEntities())
                {
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var functions = db.TB_SYS_FUNCTION.OrderBy(f => f.SORT).ToList();
                    return Ok(functions);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception("Lỗi khi tải danh mục chức năng: " + ex.Message, ex));
            }
        }
    }

    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public bool IsGroup { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public bool? Disabled { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
    }

    public class SaveRightsRequest
    {
        public List<string> FunctionCodes { get; set; }
        public List<string> FuncCodes { get; set; }

        public List<string> GetEffectiveCodes()
        {
            return FunctionCodes ?? FuncCodes ?? new List<string>();
        }
    }
}
