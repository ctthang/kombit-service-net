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

        public static Uri StsCertificateEndpointUri
        {
            get { return new Uri(StsBaseAddress + ConfigurationManager.AppSettings["StsCertificateEndpoint"]); }
        }

        public static Uri StsMexEndpointUri
        {
            get { return new Uri(StsBaseAddress + ConfigurationManager.AppSettings["StsMexEndpoint"]); }
        }

        public static X509Certificate2 StsServiceCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["StsServiceCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static string StsServiceEndpointIdentity
        {
            get { return ConfigurationManager.AppSettings["StsServiceCertificateDNSIdentity"]; }
        }

        public static Uri ServiceAddressUri
        {
            get { return new Uri(ServiceBaseAddress + ConfigurationManager.AppSettings["ServiceAddress"]); }
        }

        public static X509Certificate2 ServiceServiceCertificate
        {
            get
            {
                string certificateThumbprint =
                    ConfigurationManager.AppSettings["ServiceServiceCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static string ServiceServiceEndpointIdentity
        {
            get { return ConfigurationManager.AppSettings["ServiceServiceCertificateDNSIdentity"]; }
        }

        public static X509Certificate2 ClientCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AValidClientCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static X509Certificate2 AValidOnBehalfOfCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["AValidOnBehalfOfCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

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