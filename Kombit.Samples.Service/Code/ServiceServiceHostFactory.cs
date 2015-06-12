#region

using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Kombit.Samples.Common.Binding;
using Kombit.Samples.Common.Service.ServiceInterfaces;

#endregion

namespace Kombit.Samples.Service.Code
{
    /// <summary>
    ///     Factory that provides instances of ServiceHost in managed hosting environments that service is running
    /// </summary>
    public class ServiceServiceHostFactory : ServiceHostFactoryBase
    {
        /// <summary>
        ///     Create a service host which will run service service
        /// </summary>
        /// <param name="constructorString"></param>
        /// <param name="baseAddresses"></param>
        /// <returns></returns>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            var host = new ServiceHost(typeof (Samples.Service.Code.Service), baseAddresses);
            AddMetadataEndpoint(host);

            var credential = new ServiceCredentials();
            credential.ServiceCertificate.Certificate = Constants.ServiceServiceCertificate;
            credential.UseIdentityConfiguration = true;
            credential.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.None;
            credential.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            host.Description.Behaviors.Add(credential);

            //Add soap binding endpoint
            //var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            //binding.Security.Message.IssuedKeyType = SecurityKeyType.AsymmetricKey;
            //binding.Security.Message.IssuedTokenType =
            //    "http://docs.oasis-open.org/wss/oasis-wss-saml-token-profile-1.1#SAMLV2.0";
            //binding.Security.Message.EstablishSecurityContext = true;

            //var binding = new MutualIssuedTokenWithMessageSecurityBinding(Constants.HeaderSigningAlgorithm);
            var binding = new MutualIssuedTokenWithMessageSecurityBinding();

            var endpoint = host.AddServiceEndpoint(typeof (IService), binding, "");
            endpoint.Behaviors.Add(new HookLogMessageBehavior());
            endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;

            EnableIncludeExceptionInFault(host);

            var idConfig = new IdentityConfiguration();
            //Add a custom saml2securitytokenhandler to its configuration
            idConfig.SecurityTokenHandlers.AddOrReplace(new CustomSaml2SecurityTokenHandler());

            //Add audience restriction to its configuration
            var audience = Constants.ServiceAddress.TrimEnd(new[] {'/'});
            if (!idConfig.AudienceRestriction.AllowedAudienceUris.Contains(new Uri(audience)))
            {
                idConfig.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audience));
                idConfig.AudienceRestriction.AllowedAudienceUris.Add(new Uri(audience + "/"));
            }
            //Add STS trusted issuer that will be used to secure this service
            var nameRegistry = new ConfigurationBasedIssuerNameRegistry();
            nameRegistry.AddTrustedIssuer(Constants.StsSigningCertificate.Thumbprint,
                Constants.StsSigningCertificate.SubjectName.Name);
            idConfig.IssuerNameRegistry = nameRegistry;

            host.Credentials.IdentityConfiguration = idConfig;
            return host;
        }

        /// <summary>
        ///     Enable metadata endpoint for this service
        /// </summary>
        /// <param name="host"></param>
        private static void AddMetadataEndpoint(ServiceHost host)
        {
            var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(metadataBehavior);
            }
            metadataBehavior.HttpGetEnabled = true;
            metadataBehavior.HttpsGetEnabled = false;
            metadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            host.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexHttpBinding(),
                "mex");
        }

        /// <summary>
        ///     Enable includeExceptionInfault option which will add details exception in response
        /// </summary>
        /// <param name="host"></param>
        private void EnableIncludeExceptionInFault(ServiceHost host)
        {
            var debug = host.Description.Behaviors.Find<ServiceDebugBehavior>();

            // if not found - add behavior with setting turned on 
            if (debug == null)
            {
                host.Description.Behaviors.Add(
                    new ServiceDebugBehavior() {IncludeExceptionDetailInFaults = true});
            }
            else
            {
                // make sure setting is turned ON
                if (!debug.IncludeExceptionDetailInFaults)
                {
                    debug.IncludeExceptionDetailInFaults = true;
                }
            }
        }
    }
}