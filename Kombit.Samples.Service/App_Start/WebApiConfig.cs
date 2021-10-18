#region

using Kombit.Samples.Service.Code;
using System.Web.Http;

#endregion

namespace Kombit.Samples.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            GlobalConfiguration.Configuration.MessageHandlers.Add(
                new JwtHttpAuthenticationHandler());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );
        }
    }
}