using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web;

namespace Kombit.Samples.Rest
{
    /// <summary>
    /// This is an example of JwtSecurityTokenHandler to validate the "x5t#S256" confirmation
    /// </summary>
    public class ConfirmationJwtSecurityTokenHandler : JwtSecurityTokenHandler
    {
        public ClaimsPrincipal ValidateToken(HttpRequestMessage request, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (request.Headers.Authorization == null)
            {
                throw new ArgumentException("Authorization header is null");
            }

            var token = request.Headers.Authorization.Parameter;
            var claimsPrincipal = ValidateToken(token, validationParameters, out validatedToken);
            var confirmationClaim = claimsPrincipal.Claims.FirstOrDefault(c => c.Type == "cnf");
            if (confirmationClaim != null)
            {
                var clientCertificate = request.GetClientCertificate();
                if (clientCertificate == null)
                {
                    throw new ArgumentException("Client certificate is null");
                }

                var claimJObjectValue = JObject.Parse(confirmationClaim.Value);
                var x5tS256Value = claimJObjectValue.SelectToken("x5t#S256").ToString();

                var hashedThumbprint = Base64UrlEncoder.Encode(clientCertificate.GetCertHash(HashAlgorithmName.SHA256));
                if (hashedThumbprint != x5tS256Value)
                {
                    throw new SecurityTokenValidationException("Failed to validate the confirmatin claim.");
                }
            }

            return claimsPrincipal;
        }
    }
}