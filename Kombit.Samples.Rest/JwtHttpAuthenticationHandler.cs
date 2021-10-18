using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using Kombit.Samples.Common;

namespace Kombit.Samples.Rest
{
    public class JwtHttpAuthenticationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!CanHandle(request)) return base.SendAsync(request, cancellationToken);

            try
            {
                ClaimsPrincipal claimsPrincipal = Validate(request);
                if (!claimsPrincipal.Identity.IsAuthenticated)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                    var tsc = new TaskCompletionSource<HttpResponseMessage>();
                    tsc.SetResult(response);
                    return tsc.Task;
                }

                SetPrincipal(request, claimsPrincipal);
            }
            catch (SecurityTokenValidationException ex)
            {
                return HandleExceptionResponse(request, ex);
            }
            catch (ArgumentException ex)
            {
                return HandleExceptionResponse(request, ex);
            }
            catch (Exception ex)
            {
                return HandleExceptionResponse(request, ex);
            }

            return SendAsyncCore(request, cancellationToken);
        }

        protected virtual Task<HttpResponseMessage> SendAsyncCore(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }

        private static Task<HttpResponseMessage> HandleExceptionResponse(HttpRequestMessage request, Exception ex, HttpStatusCode statusCode = HttpStatusCode.Unauthorized)
        {
            var tsc = new TaskCompletionSource<HttpResponseMessage>();

            var requestId = HttpContext.Current.Items["RequestId"] + string.Empty;
            var faultMessage = "Security Token Invalid";
            var errorResponse = new ErrorResponse((int)statusCode, faultMessage);

            tsc.SetResult(request.CreateResponse(statusCode, errorResponse));
            return tsc.Task;
        }

        private static bool CanHandle(HttpRequestMessage request)
        {
            return request.RequestUri.AbsolutePath.IndexOf("/api/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private ClaimsPrincipal Validate(HttpRequestMessage request)
        {
            if (request.Headers.Authorization == null)
            {
                throw new ArgumentException("Authorization is required");
            }

            string authorizationHeader = request.Headers.Authorization.Scheme;
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                throw new ArgumentException("Authorization is required");
            }

            var tokenHandler = CreateJwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = Constants.JwtIssuer,
                ValidateAudience = false,
                IssuerSigningKey = new X509SecurityKey(JwtSigningCertificate)
            };

            SecurityToken securityToken;
            var claimsPrincipal = tokenHandler.ValidateToken(request.Headers.Authorization.Parameter, tokenValidationParameters, out securityToken);
            if (claimsPrincipal == null)
            {
                throw new SecurityTokenValidationException("claimsPrincipal or accessToken is invalid");
            }

            return claimsPrincipal;
        }

        public static X509Certificate2 JwtSigningCertificate
        {
            get
            {
                return CertificateLoader.LoadCertificateFromMyStore(Constants.JwtSigningCertificate);
            }
        }

        protected virtual JwtSecurityTokenHandler CreateJwtSecurityTokenHandler()
        {
            return new JwtSecurityTokenHandler();
        }

        protected virtual void SetPrincipal(HttpRequestMessage request, ClaimsPrincipal principal)
        {
            if (principal.Identity.IsAuthenticated)
            {
                string name = "(anonymous)";

                if (!string.IsNullOrWhiteSpace(principal.Identity.Name))
                {
                    name = principal.Identity.Name;
                }
                else if (principal.Claims.First() != null)
                {
                    name = principal.Claims.First().Value;
                }
            }

            Thread.CurrentPrincipal = principal;

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = principal;
            }
        }
    }
}