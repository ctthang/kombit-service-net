using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombit.Samples.JwtConsumer
{
    public class AccessTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        // The lifetime in seconds of the access token.
        [JsonProperty(PropertyName = "expires_in")]
        public double ExpiresIn { get; set; }
    }
}
