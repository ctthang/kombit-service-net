#region

using System.ServiceModel.Activation;
using System.Web.Mvc;
using System.Web.Routing;
using Kombit.Samples.Common.Service.ServiceInterfaces;
using Kombit.Samples.Service.Code;

#endregion

namespace Kombit.Samples.Service
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.Add(new ServiceRoute(
                "kombit/service",
                new ServiceServiceHostFactory(),
                typeof (IService)));
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional}
                );
        }
    }
}