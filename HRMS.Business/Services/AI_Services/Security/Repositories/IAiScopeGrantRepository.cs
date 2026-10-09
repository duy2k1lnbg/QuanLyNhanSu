using System;
using System.Collections.Generic;

namespace Bu.Services.AI_Services.Security
{
    /// <summary>
    /// Ngữ cảnh bảo mật xác thực danh tính và quyền quản trị viên của người thao tác (Actor)
    /// </summary>
    public interface IAdminSecurityContext
    {
        bool IsAdmin();
        int GetActorUserId();
        string GetActorUsername();
        string GetClientType();
    }

    /// <summary>
    /// Ngữ cảnh bảo mật trên Desktop: kiểm tra trực tiếp qua UserSession
    /// </summary>
    public class DesktopAdminSecurityContext : IAdminSecurityContext
    {
        public bool IsAdmin()
        {
            return Bu.CLASS_SYSTEM.UserSession.IsAdmin;
        }

        public int GetActorUserId()
        {
            return Bu.CLASS_SYSTEM.UserSession.CurrentUser != null
                ? (int)Bu.CLASS_SYSTEM.UserSession.CurrentUser.IDUSER
                : 0;
        }

        public string GetActorUsername()
        {
            return Bu.CLASS_SYSTEM.UserSession.CurrentUser?.USERNAME;
        }

        public string GetClientType()
        {
            return "DESKTOP";
        }
    }

    /// <summary>
    /// Ngữ cảnh bảo mật trên Web API: trích xuất từ JWT Claims đã xác thực
    /// </summary>
    public class ApiAdminSecurityContext : IAdminSecurityContext
    {
        private readonly int _actorUserId;
        private readonly string _actorUsername;
        private readonly bool _isAdmin;

        public ApiAdminSecurityContext(int actorUserId, string actorUsername, bool isAdmin)
        {
            _actorUserId = actorUserId;
            _actorUsername = actorUsername;
            _isAdmin = isAdmin;
        }

        public bool IsAdmin() => _isAdmin;
        public int GetActorUserId() => _actorUserId;
        public string GetActorUsername() => _actorUsername;
        public string GetClientType() => "WEB_API";
    }

    /// <summary>
    /// Repository trừu tượng hóa toàn bộ thao tác dữ liệu chính sách AI Scope Grant
    /// Cho phép tách biệt kiểm thử độc lập mà không cần kết nối Oracle thật
    /// </summary>
    public interface IAiScopeGrantRepository
    {
        /// <summary>
        /// Kiểm tra schema CSDL đã sẵn sàng cho phân quyền AI hay chưa (TB_AI_SCOPE_GRANT, TB_AI_REVISION)
        /// </summary>
        bool IsSchemaReady(out string notReadyReason);

        /// <summary>
        /// Xác thực đối tượng (User hoặc Group) có tồn tại trong hệ thống hay không
        /// </summary>
        bool SubjectExists(string subjectType, int subjectId, out string code, out string name, out int? manv, out bool isDisabled);

        /// <summary>
        /// Lấy danh sách ID các nhóm mà User là thành viên
        /// </summary>
        List<int> GetUserGroupIds(int userId);

        /// <summary>
        /// Lấy tên nhóm quyền theo Group ID
        /// </summary>
        string GetGroupName(int groupId);

        /// <summary>
        /// Lấy danh sách quyền chức năng nghiệp vụ của User (kết hợp trực tiếp và từ các nhóm)
        /// Tiêu chí: CAN_VIEW = 1 OR USER_RIGHT = 1 (đồng bộ với PKG_AI_AUTH)
        /// </summary>
        HashSet<string> GetUserFunctionRights(int userId, IEnumerable<int> groupIds);

        /// <summary>
        /// Lấy danh sách quyền chức năng nghiệp vụ của Nhóm
        /// </summary>
        HashSet<string> GetGroupFunctionRights(int groupId);

        /// <summary>
        /// Lấy danh sách các grant trực tiếp đã cấu hình cho một đối tượng
        /// </summary>
        List<AiScopeGrantRecord> GetGrants(string subjectType, int subjectId);

        /// <summary>
        /// Lấy danh sách phòng ban hợp lệ
        /// </summary>
        List<KeyValuePair<string, string>> GetDepartmentOptions();

        /// <summary>
        /// Lấy danh sách công ty hợp lệ
        /// </summary>
        List<KeyValuePair<string, string>> GetCompanyOptions();

        /// <summary>
        /// Lấy phiên bản chính sách hiện hành (Revision Number)
        /// </summary>
        long GetCurrentPolicyRevision();

        /// <summary>
        /// Lưu cấu hình phân quyền AI của subject có đảm bảo tính toàn vẹn Transaction và Optimistic Concurrency
        /// </summary>
        bool SaveGrantsTransactional(
            string subjectType,
            int subjectId,
            List<AiScopeGrantRecord> grantsToSave,
            long expectedRevision,
            int actorUserId,
            string actorUsername,
            string clientType,
            out long newRevision,
            out int affectedCount,
            out string errorMessage
        );
    }
}
