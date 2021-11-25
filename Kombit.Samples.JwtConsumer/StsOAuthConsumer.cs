using Newtonsoft.Json;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Xunit;

namespace Kombit.Samples.JwtConsumer
{
    public class StsOAuthConsumer
    {
        public AccessTokenResponse SendRESTOAuthRequest(string appliesTo, string scope, out OAuthErrorResponse errorResponse, string grantType = "client_credentials")
        {
            var url = $"{Constants.StsOAuthEndpointUri.AbsoluteUri}?client_id={appliesTo}&grant_type={grantType}&scope={scope}";
            var client = new ApiWebRequest(Constants.ClientCertificate);
            var httpResponse = client.Post(url);
            var responseMessage = httpResponse.Content.ReadAsStringAsync().Result;
            errorResponse = null;
            if (!httpResponse.IsSuccessStatusCode)
            {
                errorResponse = JsonConvert.DeserializeObject<OAuthErrorResponse>(responseMessage);
                return null;
            }

            return JsonConvert.DeserializeObject<AccessTokenResponse>(responseMessage);
        }

        /// <summary>
        ///     An integration test to show up how to request an OAuth token
        /// </summary>
        [Fact]
        public void SendOAuthRequestSuccessfully()
        {
            var token = SendRESTOAuthRequest(Constants.StsOAuthClientId, Constants.StsOAuthScope, out _);

            //Verify the issued token responsed from STS service
            Assert.True(token != null);
            ValidateToken(token, Constants.StsOAuthScope);
        }

        [Fact]
        public void SendOAuthObORequestSuccessfully()
        {
            var token = SendRESTOAuthRequest(Constants.StsOAuthClientId, Constants.StsOAuthScopeWithObO, out _);

            //Verify the issued token responsed from STS service
            Assert.True(token != null);
            ValidateToken(token, Constants.StsOAuthScopeWithObO);
        }

        [Fact]
        public void SendOAuthRequestAndCallRESTAPISuccessfully()
        {
            var token = SendRESTOAuthRequest(Constants.StsOAuthClientId, Constants.StsOAuthScope, out _);

            //Verify the issued token responsed from STS service
            Assert.True(token != null);
            var accessToken = token.AccessToken;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                        accessToken);

                var responseMessage = client.GetAsync(Constants.TestRestAPIEndpoint);

                var result = responseMessage.Result.Content.ReadAsStringAsync().Result;

                Assert.Contains("Congratulations!", result);
            }
        }

        [Fact]
        public void SendOAuthRequestInvalidClientIdError()
        {
            SendRESTOAuthRequest(string.Empty, Constants.StsOAuthScope, out OAuthErrorResponse errorResponse);

            //Verify the error responsed from STS service
            Assert.NotNull(errorResponse);
            Assert.Equal("invalid_client", errorResponse.Error);
        }

        [Theory(DisplayName = "Invalid scope")]
        [InlineData("entityid:,anvenderkontekst:12345678")]
        [InlineData("entityid:https://test.service.com/serviceid.svc,anvenderkontekst:")]
        public void SendOAuthRequestInvalidScopeError(string scope)
        {
            SendRESTOAuthRequest(Constants.StsOAuthClientId, scope, out OAuthErrorResponse errorResponse);

            //Verify the error responsed from STS service
            Assert.NotNull(errorResponse);
            Assert.Equal("invalid_scope", errorResponse.Error);
        }

        private Dictionary<string, string> ValidateToken(AccessTokenResponse tokenResponse, string scope)
        {
            Assert.Equal("bearer", tokenResponse.TokenType);
            var claims = GetTokenInfo(tokenResponse.AccessToken);

            Assert.NotNull(claims["cvr"]);
            Assert.NotNull(claims["spec_ver"]);
            Assert.NotNull(claims["priv"]);
            Assert.NotNull(claims["iss"]);

            Assert.Contains("anvenderkontekst:" + claims["cvr"], scope);

            return claims;
        }

        protected Dictionary<string, string> GetTokenInfo(string token)
        {
            var tokenInfo = new Dictionary<string, string>();

            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(token);
            var claims = jwtSecurityToken.Claims.ToList();

            foreach (var claim in claims)
            {
                tokenInfo.Add(claim.Type, claim.Value);
            }

            return tokenInfo;
        }
    }
}
