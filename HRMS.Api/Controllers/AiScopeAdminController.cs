using System;
using System.Net;
using System.Web.Http;
using Bu.Services.AI_Services.Security;
using HRMS_API.Filters;

namespace HRMS_API.Controllers
{
    /// <summary>
    /// API Quản trị phân quyền tra cứu AI (Scope Grants) cho User và Group
    /// Yêu cầu xác thực tài khoản Quản trị viên (Admin)
    /// </summary>
    [JwtAuthorize(RequireAdmin = true)]
    [RoutePrefix("api/admin/ai-scope")]
    public class AiScopeAdminController : ApiController
    {
        private readonly IAiScopeGrantManagementService _injectedScopeService;

        public AiScopeAdminController() : this(null)
        {
        }

        public AiScopeAdminController(IAiScopeGrantManagementService scopeService)
        {
            _injectedScopeService = scopeService;
        }

        private IAiScopeGrantManagementService GetService()
        {
            if (_injectedScopeService != null) return _injectedScopeService;

            var jwtUser = JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            int actorId = 0;
            string actorUsername = null;
            bool isAdmin = false;

            if (jwtUser != null)
            {
                int.TryParse(jwtUser.UserId, out actorId);
                actorUsername = jwtUser.Username;
                isAdmin = jwtUser.IsAdmin;
            }

            var secCtx = new ApiAdminSecurityContext(actorId, actorUsername, isAdmin);
            return new AiScopeGrantManagementService(new OracleAiScopeGrantRepository(), secCtx);
        }

        /// <summary>
        /// GET: api/admin/ai-scope?subjectType=USER&subjectId=123
        /// Lấy chi tiết ma trận quyền tra cứu AI cho tài khoản hoặc nhóm
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetSubjectScopeOverview(string subjectType, int subjectId)
        {
            if (subjectId <= 0)
            {
                return BadRequest("Mã đối tượng (subjectId) không hợp lệ.");
            }

            string sType = (subjectType ?? "USER").Trim().ToUpperInvariant();
            if (sType != "USER" && sType != "GROUP")
            {
                return BadRequest("Loại đối tượng (subjectType) phải là USER hoặc GROUP.");
            }

            try
            {
                var svc = GetService();
                var overview = svc.GetSubjectScopeOverview(sType, subjectId);
                return Ok(new { success = true, data = overview });
            }
            catch (UnauthorizedAccessException uex)
            {
                return Content(HttpStatusCode.Forbidden, new
                {
                    success = false,
                    message = uex.Message
                });
            }
            catch (InvalidOperationException ioex)
            {
                return Content(HttpStatusCode.NotFound, new
                {
                    success = false,
                    message = ioex.Message
                });
            }
            catch (Exception ex)
            {
                string correlationId = Guid.NewGuid().ToString("N");
                System.Diagnostics.Trace.TraceError($"[GetScopeGrants Error - CorrelationId: {correlationId}]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi tải thông tin phân quyền AI. Vui lòng liên hệ quản trị viên.",
                    correlationId = correlationId
                });
            }
        }

        /// <summary>
        /// GET: api/admin/ai-scope/options
        /// Lấy danh mục phòng ban và công ty phục vụ chọn phạm vi
        /// </summary>
        [HttpGet]
        [Route("options")]
        public IHttpActionResult GetScopeOptions()
        {
            try
            {
                var svc = GetService();
                var depts = svc.GetDepartmentOptions();
                var comps = svc.GetCompanyOptions();
                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        departments = depts,
                        companies = comps
                    }
                });
            }
            catch (UnauthorizedAccessException uex)
            {
                return Content(HttpStatusCode.Forbidden, new
                {
                    success = false,
                    message = uex.Message
                });
            }
            catch (Exception ex)
            {
                string correlationId = Guid.NewGuid().ToString("N");
                System.Diagnostics.Trace.TraceError($"[GetScopeOptions Error - CorrelationId: {correlationId}]: " + ex);
                return Content(HttpStatusCode.InternalServerError, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi tải danh mục phạm vi. Vui lòng liên hệ quản trị viên.",
                    correlationId = correlationId
                });
            }
        }

        /// <summary>
        /// POST: api/admin/ai-scope
        /// Lưu cấu hình quyền tra cứu AI, ghi nhật ký thay đổi và cập nhật phiên bản chính sách
        /// </summary>
        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveSubjectScopeGrants([FromBody] SaveAiSubjectScopeGrantsRequest request)
        {
            if (request == null)
            {
                return BadRequest("Dữ liệu yêu cầu không hợp lệ.");
            }

            var svc = GetService();
            var result = svc.SaveSubjectScopeGrants(request);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.IsConcurrencyConflict)
            {
                return Content(HttpStatusCode.Conflict, result);
            }

            if (result.IsSchemaNotReady)
            {
                return Content(HttpStatusCode.ServiceUnavailable, result);
            }

            return Content(HttpStatusCode.BadRequest, result);
        }
    }
}
