using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HRMS_API
{
    /// <summary>
    /// Message Handler xử lý CORS an toàn cho ASP.NET Web API 2
    /// Hỗ trợ Preflight (OPTIONS), allowlist origin chặt chẽ và khai báo đầy đủ headers
    /// </summary>
    public class CorsHandler : DelegatingHandler
    {
        private static readonly string[] DefaultAllowedOrigins = new[]
        {
            "http://localhost:5173",
            "http://localhost:3000",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:3000"
        };

        private const string AllowedHeaders = "Content-Type, Authorization, Accept, X-Requested-With, X-Platform, X-Device-Id, X-Device-Name, X-Correlation-Id";
        private const string ExposeHeaders = "Retry-After, X-Correlation-Id";
        private const string AllowedMethods = "GET, POST, PUT, DELETE, OPTIONS";

        private static bool IsOriginAllowed(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin) || origin.Equals("null", StringComparison.OrdinalIgnoreCase))
                return false;

            string configOrigins = ConfigurationManager.AppSettings["Cors:AllowedOrigins"];
            if (!string.IsNullOrWhiteSpace(configOrigins))
            {
                var origins = configOrigins.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(o => o.Trim().TrimEnd('/'));
                if (origins.Any(o => string.Equals(o, origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase)))
                    return true;
            }

            return DefaultAllowedOrigins.Any(o => string.Equals(o, origin.TrimEnd('/'), StringComparison.OrdinalIgnoreCase));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string origin = request.Headers.Contains("Origin")
                ? request.Headers.GetValues("Origin").FirstOrDefault()
                : null;

            bool isCorsRequest = !string.IsNullOrWhiteSpace(origin);
            bool isOriginAllowed = isCorsRequest && IsOriginAllowed(origin);
            bool isPreflightRequest = isCorsRequest && request.Method == HttpMethod.Options;

            if (isPreflightRequest)
            {
                var response = new HttpResponseMessage(isOriginAllowed ? HttpStatusCode.OK : HttpStatusCode.Forbidden);
                if (isOriginAllowed)
                {
                    response.Headers.Add("Access-Control-Allow-Origin", origin);
                    response.Headers.Add("Vary", "Origin");
                    response.Headers.Add("Access-Control-Allow-Methods", AllowedMethods);
                    response.Headers.Add("Access-Control-Allow-Headers", AllowedHeaders);
                    response.Headers.Add("Access-Control-Expose-Headers", ExposeHeaders);
                    response.Headers.Add("Access-Control-Max-Age", "86400");
                }
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(t =>
            {
                var response = t.Result;
                if (isCorsRequest && isOriginAllowed && !response.Headers.Contains("Access-Control-Allow-Origin"))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", origin);
                    response.Headers.Add("Vary", "Origin");
                    response.Headers.Add("Access-Control-Expose-Headers", ExposeHeaders);
                }
                return response;
            }, cancellationToken);
        }
    }
}
