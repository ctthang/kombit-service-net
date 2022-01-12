using Kombit.Samples.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Kombit.Samples.JwtConsumer
{
    public static class Constants
    {
        public static string StsBaseAddress = ConfigurationManager.AppSettings["StsBaseAddress"];
        /// <summary>
        /// Indicate sts oauth issue endpoint address
        /// </summary>
        public static Uri StsOAuthEndpointUri
        {
            get { return new Uri(StsBaseAddress + ConfigurationManager.AppSettings["StsOAuthEndpoint"]); }
        }

        public static string StsOAuthClientId
        {
            get { return ConfigurationManager.AppSettings["StsOAuthClientId"]; }
        }

        public static string StsOAuthScope
        {
            get { return ConfigurationManager.AppSettings["StsOAuthScope"]; }
        }

        public static string StsOAuthScopeWithObO
        {
            get { return ConfigurationManager.AppSettings["StsOAuthScopeWithObO"]; }
        }

        public static Uri TestRestAPIEndpoint
        {
            get
            {
                return new Uri(ConfigurationManager.AppSettings["TestRestAPIEndpoint"]);
            }
        }
        
        public static X509Certificate2 ClientCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AValidClientCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static X509Certificate2 ClientCertificate2
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AValidClientCertificateThumbprint2"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }
    }
}
