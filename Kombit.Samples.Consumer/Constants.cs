using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using Kombit.Samples.Common;

namespace Kombit.Samples.Consumer
{
    public static class Constants
    {
#if DEBUG
        //public const string StsBaseAddress = "https://localhost:44301/";
        public static string StsBaseAddress = ConfigurationManager.AppSettings["StsBaseAddress"];
#endif
#if !DEBUG
        public static string StsBaseAddress = ConfigurationManager.AppSettings["StsBaseAddress"];
#endif
#if DEBUG
        //public const string ServiceBaseAddress = "https://localhost:44302/";
        public static string ServiceBaseAddress = ConfigurationManager.AppSettings["ServiceBaseAddress"];
#endif
#if !DEBUG
        public static string ServiceBaseAddress = ConfigurationManager.AppSettings["ServiceBaseAddress"];
#endif

        public static string BootstraptokenServiceBaseAddress =
            ConfigurationManager.AppSettings["BootstraptokenServiceBaseAddress"];
        public static SigningAlgorithmMethod HeaderSigningAlgorithm
        {
            get
            {
                var algorithm = ConfigurationManager.AppSettings["HeaderSigningAlgorithm"];
                SigningAlgorithmMethod algorithmMethod;
                return Enum.TryParse(algorithm, true, out algorithmMethod)
                    ? algorithmMethod
                    : SigningAlgorithmMethod.Sha256;
            }
        }

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
        /// <summary>
        /// Indicate sts issue endpoint address
        /// </summary>
        public static Uri StsCertificateEndpointUri
        {
            get { return new Uri(StsBaseAddress + ConfigurationManager.AppSettings["StsCertificateEndpoint"]); }
        }

        /// <summary>
        /// Indicate sts mex endpoint address
        /// </summary>
        public static Uri StsMexEndpointUri
        {
            get { return new Uri(StsBaseAddress + ConfigurationManager.AppSettings["StsMexEndpoint"]); }
        }

        /// <summary>
        /// Indicate sts service certificate
        /// </summary>
        public static X509Certificate2 StsServiceCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["StsServiceCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        /// <summary>
        /// Indicate sts service endpoint's identity
        /// </summary>
        public static string StsServiceEndpointIdentity
        {
            get { return ConfigurationManager.AppSettings["StsServiceCertificateDNSIdentity"]; }
        }

        /// <summary>
        /// Indicate service address which will is secured by the issued token
        /// </summary>
        public static Uri ServiceAddressUri
        {
            get { return new Uri(ServiceBaseAddress + ConfigurationManager.AppSettings["ServiceAddress"]); }
        }

        /// <summary>
        /// Indicate service certificate's thumbprint
        /// </summary>
        public static X509Certificate2 ServiceServiceCertificate
        {
            get
            {
                string certificateThumbprint =
                    ConfigurationManager.AppSettings["ServiceServiceCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        /// <summary>
        /// Indicate service endpoint's identity
        /// </summary>
        public static string ServiceServiceEndpointIdentity
        {
            get { return ConfigurationManager.AppSettings["ServiceServiceCertificateDNSIdentity"]; }
        }


        /// <summary>
        /// A client certificate used for requesting to sts service
        /// </summary>
        public static X509Certificate2 ClientCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AValidClientCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        /// <summary>
        /// A certificate used for requesting a bootstrap token from sts 
        /// </summary>
        public static X509Certificate2 AValidOnBehalfOfCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AValidOnBehalfOfCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

         /// <summary>
        /// A client certificate used for requesting to sts service
        /// </summary>
        public static X509Certificate2 AnInvalidClientCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AnInvalidClientCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        
        /// <summary>
        /// A avender context of the authenticated user
        /// </summary>
        public static string AnvenderContext
        {
            get { return ConfigurationManager.AppSettings["AnvenderContext"]; }
        }

        public static string BppValue
        {
            get { return ConfigurationManager.AppSettings["BppValue"]; }
        }

        public static string ExpectedResponseMessage
        {
            get { return ConfigurationManager.AppSettings["ExpectedResponseMessage"]; }
        }

        public static Boolean EnableSoapMessageLog
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSoapMessageLog"]); }
        }
    }
}