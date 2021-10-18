using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombit.Samples.Consumer
{
    public class AccessTokenResponse
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        // https://datatracker.ietf.org/doc/html/rfc6749#section-7.1
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get { return "bearer"; } }

        // The lifetime in seconds of the access token.
        [JsonProperty(PropertyName = "expires_in")]
        public double ExpiresIn { get; set; }
    }
}
