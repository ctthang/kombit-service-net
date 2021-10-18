using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Kombit.Samples.Service.Code
{
    /// <summary>
    /// This demo authorization filter will veriry the Cvr number from the Access token against the configured value
    /// </summary>
    public class DemoRestApiAuthorizationFilter : AuthorizationFilterAttribute
    {
        public string Cvr
        {
            get { return Constants.JwtCvr; }
        }

        private HttpContextBase currentContext;

        public HttpContextBase CurrentContext
        {
            get
            {
                if (currentContext == null)
                {
                    return new HttpContextWrapper(HttpContext.Current);
                }

                return currentContext;
            }
        }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;

            if (!principal.Identity.IsAuthenticated)
            {
                return DenyRequest(actionContext);
            }
            else
            {
                var cvrClaim = principal.Claims.FirstOrDefault(x => x.Type == "cvr");
                if (cvrClaim == null || cvrClaim.Value != this.Cvr)
                {
                    return DenyRequest(actionContext);
                }
            }

            return base.OnAuthorizationAsync(actionContext, cancellationToken);
        }

        private Task DenyRequest(HttpActionContext actionContext)
        {
            var faultMessage = "Access denied.";
            var errorResponse = new ErrorResponse((int)HttpStatusCode.Forbidden, faultMessage);

            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden, errorResponse);

            return Task.FromResult<object>(null);
        }
    }
}