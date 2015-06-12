#region

using System;
using System.Configuration;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using Kombit.Samples.Common;
using Kombit.Samples.Common.Binding;

#endregion

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     Factory that provides instances of ServiceHost in managed hosting environments
    ///     where the host instance is created dynamically in response to incoming messages.
    /// </summary>
    public class WsTrustServiceHostFactory : ServiceHostFactory
    {
        public const string SecurityTokenServiceNamespace =
            "http://schemas.microsoft.com/ws/2008/06/identity/securitytokenservice";

        /// <summary>
        ///     Creates a service host to process WS-Trust 1.3 requests
        /// </summary>
        /// <param name="constructorString">The constructor string.</param>
        /// <param name="baseAddresses">The base addresses.</param>
        /// <returns>A WS-Trust ServiceHost</returns>
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            Logging.Instance.Debug("Start creating service host");
            var host = new WSTrustServiceHost(new WsTrustServiceConfiguration(), baseAddresses);
            EnableMexEndpoint(baseAddresses, host);
            EnableIncludeExceptionInFault(host);
            var credential = new ServiceCredentials();
            credential.ServiceCertificate.Certificate = Constants.StsServiceCertificate;
            host.Description.Behaviors.Add(credential);
            // Configure ChainTrust with no revocation check.
            X509ClientCertificateAuthentication myAuthProperties = host.Credentials.ClientCertificate.Authentication;
            myAuthProperties.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            myAuthProperties.RevocationMode = X509RevocationMode.NoCheck;
            GenerateEndpoints(host);
            return host;
        }

        /// <summary>
        ///     This method is to enable IncludeExceptionInFault in response to client
        /// </summary>
        /// <param name="host"></param>
        private void EnableIncludeExceptionInFault(WSTrustServiceHost host)
        {
            ServiceDebugBehavior debug = host.Description.Behaviors.Find<ServiceDebugBehavior>();

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

        /// <summary>
        ///     This method is to enable mex endpoint
        /// </summary>
        /// <param name="baseAddresses"></param>
        /// <param name="host"></param>
        private static void EnableMexEndpoint(Uri[] baseAddresses, WSTrustServiceHost host)
        {
            var metadataBehavior = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadataBehavior == null)
            {
                metadataBehavior = new ServiceMetadataBehavior();
                host.Description.Behaviors.Add(metadataBehavior);
            }
            foreach (var baseAddress in baseAddresses)
            {
                if (baseAddress.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    metadataBehavior.HttpsGetEnabled = true;
                    metadataBehavior.HttpsGetUrl = new Uri(baseAddress.OriginalString + "/mex");
                }
                else
                {
                    metadataBehavior.HttpGetEnabled = true;
                    metadataBehavior.HttpGetUrl = new Uri(baseAddress.OriginalString + "/mex");
                }
            }
        }

        /// <summary>
        ///     This method is to generate certificate endpoint
        /// </summary>
        /// <param name="host"></param>
        protected virtual void GenerateEndpoints(WSTrustServiceHost host)
        {
            Logging.Instance.Debug("generate certificate endpoint");

            var binding = new MutualCertificateWithMessageSecurityBinding(Constants.HeaderSigningAlgorithm)
            {
                Namespace = "http://schemas.microsoft.com/ws/2008/06/identity/securitytokenservice",
            };

            int wcfServiceClockSkew = Convert.ToInt32(ConfigurationManager.AppSettings["WcfServiceClockSkew"]);
            if (wcfServiceClockSkew > 0)
            {
                binding.Elements.Find<SecurityBindingElement>().LocalServiceSettings.MaxClockSkew =
                    TimeSpan.FromSeconds(wcfServiceClockSkew);
            }

            ServiceEndpoint endpoint = host.AddServiceEndpoint(
                typeof (IWSTrust13SyncContract),
                binding,
                "/certificate");
            foreach (var operation in endpoint.Contract.Operations)
            {
                if (operation.Name == "Trust13Issue")
                {
                    var faultDescription = new FaultDescription(operation.Name)
                    {
                        Name = "StsFaultMessage",
                        DetailType = typeof(StsFaultMessage),
                        Namespace = "http://kombit.sample.dk/fault",
                        ProtectionLevel = ProtectionLevel.Sign
                    };
                    operation.Faults.Add(faultDescription);
                }
            }
            endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;
            endpoint.Behaviors.Add(new DecoratedEndpointBehavior());
            if (Constants.EnableSoapMessageLog)
                endpoint.Behaviors.Add(new HookLogMessageBehavior());

            
        }
    }
}