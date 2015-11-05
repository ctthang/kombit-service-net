#region

using System.ServiceModel.Activation;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Kombit.Samples.STS.Code;

#endregion

namespace Kombit.Samples.STS
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = ""}, // Parameter defaults
                new {controller = new IgnoreServiceRouteConstraint()}
                );

            routes.Add(new ServiceRoute(
                "kombit/sts",
                new WsTrustServiceHostFactory(),
                typeof(WsTrustServiceConfiguration)));
        }

        public class DateConstraint : IRouteConstraint
        {
            public bool Match(HttpContextBase httpContext, Route route, string parameterName,
                RouteValueDictionary values, RouteDirection routeDirection)
            {
                return !values[parameterName].ToString().StartsWith("kombit");
            }
        }
    }
}