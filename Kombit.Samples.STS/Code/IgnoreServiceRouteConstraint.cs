#region

using System;
using System.Web;
using System.Web.Routing;

#endregion

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     Igonore route map if it is not kombit endpoint
    /// </summary>
    public class IgnoreServiceRouteConstraint : IRouteConstraint
    {
        /// <summary>
        ///     Checks if a requested path matches a route
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="route"></param>
        /// <param name="parameterName"></param>
        /// <param name="values"></param>
        /// <param name="routeDirection"></param>
        /// <returns></returns>
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            return !values[parameterName].ToString().StartsWith("kombit", StringComparison.OrdinalIgnoreCase);
        }
    }
}