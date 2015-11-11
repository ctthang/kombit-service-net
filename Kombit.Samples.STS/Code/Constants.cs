#region

using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using Kombit.Samples.Common;

#endregion

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     Specify all the constants referenced in STS
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     A xml that represents to a basic privilege value
        /// </summary>
        public static string BppValue
        {
            get { return ConfigurationManager.AppSettings["BppValue"]; }
        }

        /// <summary>
        ///     a dummy appliesTo value which will notify to STS throw an exeption
        /// </summary>
        public const string DummyAppliesToError = "http://kombit.samples.local/";

        /// <summary>
        ///     a format of error message which will be responsed to client
        /// </summary>
        public const string DummyErrorMessagePattern = "There is an error '{0}' thrown from STS service, please check server log for more details.";

        public const string DummyDetailErrorMessagePattern = "This message is to illustrate how an error '{0}' is thrown.";

#if DEBUG
    /// <summary>
    /// Base address of STS service
    /// </summary>
        public const string BaseAddress = "https://localhost:44301/";
        //public static string BaseAddress = ConfigurationManager.AppSettings["BaseAddress"];
#endif
#if !DEBUG
        public static string BaseAddress = ConfigurationManager.AppSettings["BaseAddress"];
#endif

        /// <summary>
        ///     A certificate that is used to sign the message
        /// </summary>
        public static X509Certificate2 SigningCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["StsSigningCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

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

        /// <summary>
        ///     An algorithm which is used in assertion signing
        /// </summary>
        public static SigningAlgorithmMethod SigningAlgorithm
        {
            get
            {
                var algorithm = ConfigurationManager.AppSettings["SigningAlgorithm"];
                SigningAlgorithmMethod algorithmMethod;
                return Enum.TryParse(algorithm, true, out algorithmMethod)
                    ? algorithmMethod
                    : SigningAlgorithmMethod.Sha256;
            }
        }

        /// <summary>
        ///     A credentials that is used to sign message
        /// </summary>
        public static SigningCredentials SigningCredentials
        {
            get
            {
                if (SigningAlgorithm == SigningAlgorithmMethod.Sha256)
                    return new X509SigningCredentials(SigningCertificate,
                        "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256",
                        "http://www.w3.org/2001/04/xmlenc#sha256");
                return new X509SigningCredentials(SigningCertificate,
                    "http://www.w3.org/2000/09/xmldsig#rsa-sha1",
                    "http://www.w3.org/2000/09/xmldsig#sha1");
            }
        }

        /// <summary>
        ///     A certificate which is used as service certificate for certificate endpoint.
        /// </summary>
        public static X509Certificate2 StsServiceCertificate
        {
            get
            {
                string certificateThumbprint = ConfigurationManager.AppSettings["StsServiceCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        public static String GetSubjectNameId(X509Certificate2 cert)
        {
            return cert.Thumbprint == AValidClientCertificate.Thumbprint ? "C=DK,O=20688092,CN=Hans Jensen,Serial=123abc456" : "C=DK,O=39788093,CN=Maren Jensdatter,Serial=987abc654";
        }

        /// <summary>
        ///     A certificate which imitates a valid client certificate that is being authenticated to STS.
        /// </summary>
        public static X509Certificate2 AValidClientCertificate
        {
            get
            {
                var certificateThumbprint = ConfigurationManager.AppSettings["AValidClientCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        /// <summary>
        ///     A certificate which imitates a valid obo certificate that is being used in the current RST.
        /// </summary>
        public static X509Certificate2 AValidOnBehalfOfCertificate
        {
            get
            {
                var certificateThumbprint = ConfigurationManager.AppSettings["AValidOnBehalfOfCertificateThumbprint"];
                return CertificateLoader.LoadCertificateFromMyStore(certificateThumbprint);
            }
        }

        /// <summary>
        ///     A number indicates maximum minutes that an issued token can be alive
        /// </summary>
        public static int MaximumTokenLifetime
        {
            get
            {
                string lifeTime = ConfigurationManager.AppSettings["MaximumTokenLifetime"];
                return Convert.ToInt32(lifeTime);
            }
        }

        /// <summary>
        ///     A string which represents to a claim type of a claim that stores thumbprint of user's client certificate.
        /// </summary>
        public static string RequestorCertificateClaimType
        {
            get { return "kombit:requestor:certificate:thumbprint"; }
        }

        /// <summary>
        ///     A value that specify whether soap message log should be enabled or not
        /// </summary>
        public static Boolean EnableSoapMessageLog
        {
            get { return Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSoapMessageLog"]); }
        }
    }
}