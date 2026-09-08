using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace HRMS_API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Đảm bảo Oracle Entity Framework Provider luôn được nạp vào bộ nhớ
            Type _ = typeof(Oracle.ManagedDataAccess.EntityFramework.EFOracleProviderServices);

            // Tự động phân giải phiên bản Assembly cho System.Memory và System.Runtime.CompilerServices.Unsafe
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var asmName = new System.Reflection.AssemblyName(args.Name);
                if (asmName.Name == "System.Memory")
                {
                    string path = System.IO.Path.Combine(HttpRuntime.BinDirectory, "System.Memory.dll");
                    if (System.IO.File.Exists(path)) return System.Reflection.Assembly.LoadFrom(path);
                }
                if (asmName.Name == "System.Runtime.CompilerServices.Unsafe")
                {
                    string path = System.IO.Path.Combine(HttpRuntime.BinDirectory, "System.Runtime.CompilerServices.Unsafe.dll");
                    if (System.IO.File.Exists(path)) return System.Reflection.Assembly.LoadFrom(path);
                }
                return null;
            };

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
