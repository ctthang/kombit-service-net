using System;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using Kombit.Samples.Common;
using Kombit.Samples.Common.Binding;
using Kombit.Samples.Common.Service.ServiceInterfaces;

namespace Kombit.Samples.Consumer
{
    public class ConnectionHelper
    {
        /// <summary>
        ///     a helper method to show how send a RST to STS
        /// </summary>
        /// <param name="appliesTo"></param>
        /// <param name="onBehalfOf"></param>
        /// <param name="haveContextClaims"></param>
        /// <param name="clientCertificate"></param>
        /// <param name="messesagModifier"></param>
        /// <param name="messageEncodingElementFactory"></param>
        /// <param name="timestampDuration"></param>
        /// <returns></returns>
        public static SecurityToken SendRequestSecurityTokenRequest(string appliesTo, SecurityTokenElement onBehalfOf, X509Certificate2 clientCertificate, Func<Message, Message> messesagModifier,
            Func<MessageEncodingBindingElement> messageEncodingElementFactory = null, int timestampDuration = 0)
        {
            return SendRequestSecurityTokenRequest(appliesTo, onBehalfOf, clientCertificate, Constants.AnvenderContext, messesagModifier,
                messageEncodingElementFactory, timestampDuration);
        }

        public static SecurityToken SendRequestSecurityTokenRequest(string appliesTo, SecurityTokenElement onBehalfOf,
            X509Certificate2 clientCertificate, string cvr, Func<Message, Message> messesagModifier,
            Func<MessageEncodingBindingElement> messageEncodingElementFactory = null, int timestampDuration = 0)
        {
            var rst = new RequestSecurityToken
            {
                AppliesTo = new EndpointReference(appliesTo),
                RequestType = RequestTypes.Issue,
                TokenType = Common.WSTrust13.Constants.TokenTypes.OasisWssSaml2TokenProfile11,
                KeyType = KeyTypes.Asymmetric,
                Issuer = new EndpointReference(Constants.StsBaseAddress),
                UseKey = new UseKey(new X509SecurityToken(clientCertificate))
            };
            rst.OnBehalfOf = onBehalfOf;
            if (!string.IsNullOrEmpty(cvr))
            {
                rst.Claims.Dialect = "http://docs.oasis-open.org/wsfed/authorization/200706/authclaims";
                rst.Claims.Add(new RequestClaim("dk:gov:saml:attribute:CvrNumberIdentifier", false, cvr));
            }
            
            var client = GenerateStsCertificateClientChannel(clientCertificate, messesagModifier,
                messageEncodingElementFactory, timestampDuration);
            SecurityToken token;
            try
            {
                RequestSecurityTokenResponse rstr;
                token = client.Issue(rst, out rstr);
            }
            catch (Exception ex)
            {
                Logging.Instance.Error(ex, "There is an error responsed from ws-trust service. The test will be failed");
                throw;
            }
            return token;
            
        }

        /// <summary>
        ///     A helper method to generate channel to communicate with service service
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static IService GenerateServiceChannel(SecurityToken token)
        {
            //var binding =
            //    new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            //binding.Security.Message.EstablishSecurityContext = true;
            //binding.Security.Message.IssuedKeyType = SecurityKeyType.AsymmetricKey;
            //binding.Security.Message.IssuedTokenType = TokenTypes.OasisWssSaml2TokenProfile11;
            //var binding = new MutualIssuedTokenWithMessageSecurityBinding(Constants.HeaderSigningAlgorithm);
            var binding = new MutualIssuedTokenWithMessageSecurityBinding();

            var factory = new ChannelFactory<IService>(
                binding,
                new EndpointAddress(Constants.ServiceAddressUri,
                    EndpointIdentity.CreateDnsIdentity(Constants.ServiceServiceEndpointIdentity)));


            factory.Credentials.SupportInteractive = false;
            factory.Credentials.UseIdentityConfiguration = true;
            factory.Credentials.ServiceCertificate.ScopedCertificates.Add(
                Constants.ServiceAddressUri,
                Constants.ServiceServiceCertificate);
            factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                X509CertificateValidationMode.None;
            factory.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            factory.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;

            factory.Credentials.SecurityTokenHandlerCollectionManager[""].AddOrReplace(
                new StrReferenceSaml2SecurityTokenHandler());

            if (Constants.EnableSoapMessageLog)
                factory.Endpoint.Behaviors.Add(new HookLogServiceMessageBehavior());
            return factory.CreateChannelWithIssuedToken(token);
        }

        /// <summary>
        ///     A helper method to generate channel to communicate with STS service
        /// </summary>
        /// <param name="clientCertificate"></param>
        /// <param name="messesagModifier"></param>
        /// <param name="messageEncodingElementFactory"></param>
        /// <param name="timestampDuration"></param>
        /// <returns></returns>
        public static IWSTrustChannelContract GenerateStsCertificateClientChannel(X509Certificate2 clientCertificate,
            Func<Message, Message> messesagModifier,
            Func<MessageEncodingBindingElement> messageEncodingElementFactory = null, int timestampDuration = 0)
        {
            var stsAddress = new EndpointAddress(Constants.StsCertificateEndpointUri,
                EndpointIdentity.CreateDnsIdentity(Constants.StsServiceEndpointIdentity));
            var binding = new MutualCertificateWithMessageSecurityBinding(Constants.HeaderSigningAlgorithm,
                messageEncodingElementFactory);
            if (timestampDuration > 0)
            {
                binding.Elements.Find<SecurityBindingElement>().LocalClientSettings.TimestampValidityDuration =
                    TimeSpan.FromSeconds(timestampDuration);
            }

            var factory = new WSTrustChannelFactory(
                binding,
                stsAddress);
            factory.TrustVersion = TrustVersion.WSTrust13;

            factory.Credentials.ClientCertificate.Certificate = clientCertificate;
            factory.Credentials.ServiceCertificate.ScopedCertificates.Add(
                stsAddress.Uri,
                Constants.StsServiceCertificate);
            factory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode =
                X509CertificateValidationMode.None;
            factory.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            //Log the soap request and response to sts service
            factory.Endpoint.Contract.ProtectionLevel = ProtectionLevel.Sign;
            if (Constants.EnableSoapMessageLog)
                factory.Endpoint.Behaviors.Add(new HookLogStsMessageBehavior(messesagModifier));
            return factory.CreateChannel();
        }
    }
}