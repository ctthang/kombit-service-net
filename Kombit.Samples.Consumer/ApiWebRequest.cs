using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Kombit.Samples.Consumer
{
    public class ApiWebRequest : IDisposable
    {
        private HttpClient httpClient;
        private X509Certificate2 clientCertificate;

        public ApiWebRequest(X509Certificate2 certificate)
        {
            clientCertificate = certificate;
        }

        public ApiWebRequest()
        {

        }

        /// <summary>
        /// Method : GET
        /// </summary>
        public HttpResponseMessage Get(string path)
        {
            EnsureHttpClient();

            var t = httpClient.GetAsync(path)
                .ContinueWith(task => ValidateResult(task.Result));

            // HTTP GET
            var response = t.Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Get fail: path {0}. {1}", path, response.ReasonPhrase);
            }

            return response;
        }
        
        //1) Uninstall-Package Microsoft.Bcl.Async -Force 
        //2) Install-Package Microsoft.Bcl.Async -Version 1.0.16
        /// <summary>
        /// Method : POST
        /// </summary>
        public HttpResponseMessage Post<T>(string path, T content)
        {
            EnsureHttpClient();

            var t = httpClient.PostAsJsonAsync(path, content)
                .ContinueWith(task => ValidateResult(task.Result));

            // HTTP POST
            var response = t.Result;

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Post fail: path {0}. {1}", path, response.ReasonPhrase);
            }

            return response;
        }

        /// <summary>
        /// Initiates HttpClient to request service
        /// </summary>
        private void EnsureHttpClient()
        {
            if (httpClient != null)
                return;

            var handler = new WebRequestHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                UseProxy = false,
                ServerCertificateValidationCallback = delegate { return true; },
            };

            if (clientCertificate != null)
            {
                handler.ClientCertificates.Add(clientCertificate);
            }

            httpClient = HttpClientFactory.Create(handler);
            httpClient.BaseAddress = new Uri(Constants.StsBaseAddress);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("TransactionId", Guid.NewGuid().ToString());
            httpClient.Timeout = TimeSpan.FromSeconds(3);
        }

        /// <summary>
        /// Scan response message to write helpful message in unit test console
        /// </summary>
        private HttpResponseMessage ValidateResult(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode == false)
            {
                var errorMesg = responseMessage.Content.ReadAsStringAsync().Result;
                Console.WriteLine(errorMesg);
            }

            if (responseMessage.IsSuccessStatusCode &&
                responseMessage.Content.Headers.ContentType != null &&
                responseMessage.Content.Headers.ContentType.MediaType == "text/html")
            {
                const string errorMesg = @"
Error: IIS may ignore or reject client certificate:
Troubleshooting: 
- On IIS, click on Admin node -> select SSL Settings on the right pane -> Select Accept and then save.
- Check the client certificate thumb-print in both Identify server and client
";
                Console.WriteLine(errorMesg);
            }

            if (responseMessage.StatusCode == HttpStatusCode.Forbidden &&
                responseMessage.Content.Headers.ContentType != null &&
                responseMessage.Content.Headers.ContentType.MediaType == "text/html")
            {
                const string errorMesg = @"
Error: Client certificate might probably causing the problem.
- Troubleshooting:
    + Get 403.16 error code: the problem is probably caused by the client machine. Some issues:
        - Client cert must have private key.
        - Client cert must have be usable for client authentication. Namely, its purposes must have [Proves your identity to a remote computer].
        - CA certificate must be imported to the LocalMachine\Trusted Root Certification Authorities store of both client and server machines. Reason: both machines must agree on what CAs they trust.
        - Try to import client certificate to the LocalMachine\My store.
        - Try to import client certificate to the CurrentUser\My store. Especially if you try to call an API via browser (not via some tool like Swagger), this step is a must.
";
                Console.WriteLine(errorMesg);
            }

            return responseMessage;
        }

        /// <summary>
        /// Disposes HttpClient instance
        /// </summary>
        public void Dispose()
        {
            if (httpClient != null)
            {
                httpClient.CancelPendingRequests();
                httpClient.Dispose();
            }
        }
    }
}
