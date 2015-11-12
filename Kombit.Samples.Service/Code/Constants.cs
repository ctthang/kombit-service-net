#region

using System;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using Kombit.Samples.Common;

#endregion

namespace Kombit.Samples.Service.Code
{
    /// <summary>
    ///     A collection of all the constants used in STS Test signing service project
    /// </summary>
    public static class Constants
    {
#if DEBUG
        //public const string ServiceAddress = "https://localhost:44302/kombit/service";
        public static string ServiceAddress = ConfigurationManager.AppSettings["ServiceAddress"];
#endif
#if !DEBUG
        public static string ServiceAddress = ConfigurationManager.AppSettings["ServiceAddress"];
#endif

        /// <summary>
        ///     An algorithm which is used in message header signing
        /// </summary>
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

        public static X509Certificate2 ServiceServiceCertificate
        {
            get
            {
                string certificateThumbprint =
                    ConfigurationManager.AppSettings["ServiceServiceCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static X509Certificate2 StsSigningCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["StsSigningCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static string ResponseMessage
        {
            get { return ConfigurationManager.AppSettings["ResponseMessage"]; }
        }
    }
}