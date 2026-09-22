using HRMS_API.Services;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace HRMS_API.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Mã quyền chức năng yêu cầu (ví dụ: F_NHANSU_VIEW, F_CHAMCONG_EDIT, ...)
        /// </summary>
        public string Right { get; set; }

        /// <summary>
        /// Yêu cầu tài khoản phải là Quản trị viên tối cao (Admin)
        /// </summary>
        public bool RequireAdmin { get; set; }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            // 1. Cho phép bỏ qua xác thực nếu Action hoặc Controller có [AllowAnonymous]
            if (actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any() ||
                actionContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any())
            {
                return;
            }

            // 2. Trích xuất Authorization Header
            var authHeader = actionContext.Request.Headers.Authorization;
            if (authHeader == null || string.IsNullOrWhiteSpace(authHeader.Parameter))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { success = false, message = "Yêu cầu cần có mã xác thực (Authorization: Bearer <token>)." }
                );
                return;
            }

            string token = authHeader.Parameter;

            // 3. Kiểm tra tính toàn vẹn và giải mã JWT
            if (!JwtService.ValidateToken(token, out var claims, out var principal))
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { success = false, message = "Phiên đăng nhập không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại." }
                );
                return;
            }

            // 4. Kiểm tra trạng thái phiên làm việc phía Server (Nguồn sự thật DB)
            if (decimal.TryParse(claims.UserId, out decimal userIdVal))
            {
                var authSecurityService = new Bu.CLASS_SECURITY.AuthSecurityService();
                bool isSessionValid = authSecurityService.ValidateSession(claims.Jti, claims.TokenVersion, userIdVal);
                if (!isSessionValid)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        HttpStatusCode.Unauthorized,
                        new 
                        { 
                            success = false, 
                            code = "SESSION_REVOKED_OR_EXPIRED",
                            message = "Phiên làm việc đã bị thu hồi hoặc tài khoản đã thay đổi bảo mật. Vui lòng đăng nhập lại." 
                        }
                    );
                    return;
                }
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Unauthorized,
                    new { success = false, message = "Định danh người dùng trong Token không hợp lệ." }
                );
                return;
            }

            // 4. Kiểm tra quyền Quản trị viên (nếu có yêu cầu)
            if (RequireAdmin && !claims.IsAdmin)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Forbidden,
                    new { success = false, message = "Từ chối truy cập: Thao tác này yêu cầu quyền Quản trị viên hệ thống (Admin)." }
                );
                return;
            }

            // 5. Kiểm tra quyền chức năng cụ thể (nếu có yêu cầu)
            if (!string.IsNullOrWhiteSpace(Right) && !claims.IsAdmin)
            {
                bool hasRight = claims.Rights != null &&
                                (claims.Rights.Contains("*") || claims.Rights.Contains(Right.Trim()));

                if (!hasRight)
                {
                    actionContext.Response = actionContext.Request.CreateResponse(
                        HttpStatusCode.Forbidden,
                        new { success = false, message = $"Từ chối truy cập: Tài khoản không có mã quyền chức năng [{Right}]." }
                    );
                    return;
                }
            }

            // 6. Gán danh tính người dùng vào Request Context
            actionContext.RequestContext.Principal = principal;
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
            actionContext.Request.Properties["JwtUser"] = claims;

            // 7. Đồng bộ thông tin người dùng từ JWT vào Entity Framework Audit Logging
            if (claims != null)
            {
                if (int.TryParse(claims.UserId, out int auditUserId))
                {
                    DA.MyEntities.CurrentAuditUserId = auditUserId;
                }
                DA.MyEntities.CurrentAuditUsername = claims.Username;
            }
        }

        /// <summary>
        /// Helper lấy thông tin người dùng hiện tại từ Request đã qua xác thực
        /// </summary>
        public static JwtUserClaims GetCurrentJwtUser(HttpRequestMessage request)
        {
            if (request != null && request.Properties.TryGetValue("JwtUser", out object userObj))
            {
                return userObj as JwtUserClaims;
            }
            return null;
        }
    }
}
