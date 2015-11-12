using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Xml;
using Kombit.Samples.Common;

namespace Kombit.Samples.Service.Code
{
    /// <summary>
    ///     A custom saml2 security token handler which will do some custom validation when receiving a saml2 security token
    /// </summary>
    public class CustomSaml2SecurityTokenHandler : Saml2SecurityTokenHandler
    {
        /// <summary>
        ///     Validate security token received from client. All the custome validation code can be put here.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            //Below is just a sample code to show how a custom validation can be embedded.
            var saml2Token = token as Saml2SecurityToken;
            if (saml2Token == null)
            {
                Logging.Instance.Error("Only saml2 security token is accepted!");
                throw new ArgumentException("Only saml2 security token is accepted!");
            }
            var issuer = saml2Token.IssuerToken as X509SecurityToken;
            if (issuer == null)
            {
                Logging.Instance.Error("Only X509SecurityToken is accepted as issuer token.");
                throw new ArgumentException("Only X509SecurityToken is accepted as issuer token.");
            }
            var issuerThumprint = issuer.Certificate.Thumbprint;
            if (!issuerThumprint.Equals(Constants.StsSigningCertificate.Thumbprint))
            {
                Logging.Instance.Error("Invalid issuer.");
                throw new ArgumentException("Invalid issuer.");
            }
            return base.ValidateToken(token);
        }

        /// <summary>
        ///     It is to make a custom validate confirmation data. It must override the base class because the recipient of
        ///     confirmation data on received security token element is not null.
        /// </summary>
        /// <param name="confirmationData"></param>
        protected override void ValidateConfirmationData(Saml2SubjectConfirmationData confirmationData)
        {
            return;
            //try
            //{
            //    base.ValidateConfirmationData(confirmationData);
            //}
            //catch (Exception ex)
            //{
            //    if (!ex.Message.ToLower(CultureInfo.InvariantCulture).Contains("4157"))
            //        throw;
            //}
        }

        /// <summary>
        ///     Reads a token from a Reader. This does exactly what the Saml2SecurityTokenHandler class does, but will return an
        ///     instance of type StrTransformedSaml2SecurityToken
        ///     instead of Saml2SecurityToken
        /// </summary>
        /// <param name="reader">An xml reader that reads the token from a stream</param>
        /// <returns>An instance of type StrTransformedSaml2SecurityToken</returns>
        public override SecurityToken ReadToken(XmlReader reader)
        {
            Saml2Assertion assertion = ReadAssertion(reader);

            ReadOnlyCollection<SecurityKey> keys = ResolveSecurityKeys(assertion, Configuration.ServiceTokenResolver);

            // Resolve signing token if one is present. It may be deferred and signed by reference.
            SecurityToken issuerToken;
            TryResolveIssuerToken(assertion, Configuration.IssuerTokenResolver, out issuerToken);

            return new StrTransformedSaml2SecurityToken(assertion, keys, issuerToken);
        }
    }
}