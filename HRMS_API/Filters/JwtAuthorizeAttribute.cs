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
