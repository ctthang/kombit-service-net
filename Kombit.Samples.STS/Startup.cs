#region

using System.Web.Http;
using Owin;

#endregion

namespace Kombit.Samples.STS
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            appBuilder.UseWebApi(config);
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}