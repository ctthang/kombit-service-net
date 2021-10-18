using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombit.Samples.JwtConsumer
{
    public class OAuthErrorResponse
    {

        /// <summary>
        /// Gets or sets the "error" parameter returned to the client application.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets the "error_description" parameter returned to the client application.
        /// </summary>
        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }
    }
}
