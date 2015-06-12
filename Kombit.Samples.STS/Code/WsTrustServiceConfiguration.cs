using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
using Kombit.Samples.Common;
using Kombit.Samples.Common.WSTrust13;

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     Defines the configuration for a security token service (STS)
    /// </summary>
    public class WsTrustServiceConfiguration : SecurityTokenServiceConfiguration
    {
        /// <summary>
        ///     Initializes a new instance of the WsTrustServiceConfiguration class
        /// </summary>
        public WsTrustServiceConfiguration()
        {
            Logging.Instance.Debug("wstrust service configuration is setup");
            SetupConfiguration();
        }

        /// <summary>
        ///     Setup all the necessary settings
        /// </summary>
        private void SetupConfiguration()
        {
            SecurityTokenService = GetSecurityTokenServiceType();
            DefaultTokenLifetime = TimeSpan.FromMinutes(10);
            MaximumTokenLifetime = TimeSpan.FromMinutes(20);
            MaxClockSkew = TimeSpan.FromSeconds(100);
            DefaultTokenType = Common.WSTrust13.Constants.TokenTypes.OasisWssSaml2TokenProfile11;

            TokenIssuerName = Constants.BaseAddress;
            SigningCredentials = Constants.SigningCredentials;
            var configuration = new SecurityTokenHandlerConfiguration();
            configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;
            configuration.CertificateValidationMode = X509CertificateValidationMode.None;
            configuration.RevocationMode = X509RevocationMode.NoCheck;
            configuration.IssuerNameRegistry = new ClientCertificateIssuerNameRegistry(false, null);

            var certificateSecurityTokenHandler = new CustomCertificateSecurityTokenHandler()
            {
                Configuration = configuration
            };
            var saml2SecurityTokenHandler = new Saml2SecurityTokenHandler()
            {
                Configuration = configuration
            };

            SecurityTokenHandlers.AddOrReplace(certificateSecurityTokenHandler);
            SecurityTokenHandlers.AddOrReplace(saml2SecurityTokenHandler);

            SetupDelegationSecurityTokenHandler();
        }

        /// <summary>
        ///     This method is to setup all the necessary settings to handle OnBalfOf token element
        /// </summary>
        private void SetupDelegationSecurityTokenHandler()
        {
            var delegationHandlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();
            delegationHandlers.Configuration.AudienceRestriction.AudienceMode = AudienceUriMode.Never;
            delegationHandlers.AddOrReplace(new CustomNormalOnBehalfOfSaml2TokenHandler());
            SecurityTokenHandlerCollectionManager[SecurityTokenHandlerCollectionManager.Usage.OnBehalfOf] =
                delegationHandlers;
        }

        /// <summary>
        ///     This method is to get the security token service type
        /// </summary>
        /// <returns></returns>
        private static Type GetSecurityTokenServiceType()
        {
            return typeof (WsTrustSecurityTokenService);
        }
    }
}