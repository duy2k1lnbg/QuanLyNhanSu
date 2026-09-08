using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HRMS_API
{
    /// <summary>
    /// Message Handler xử lý CORS cho ASP.NET Web API 2
    /// Hỗ trợ cả Preflight (OPTIONS) và đính kèm Access-Control-Allow-* headers
    /// </summary>
    public class CorsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isCorsRequest = request.Headers.Contains("Origin");
            bool isPreflightRequest = isCorsRequest && request.Method == HttpMethod.Options;

            if (isPreflightRequest)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, X-Requested-With");
                var tcs = new TaskCompletionSource<HttpResponseMessage>();
                tcs.SetResult(response);
                return tcs.Task;
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(t =>
            {
                var response = t.Result;
                if (isCorsRequest && !response.Headers.Contains("Access-Control-Allow-Origin"))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
                return response;
            }, cancellationToken);
        }
    }
}
