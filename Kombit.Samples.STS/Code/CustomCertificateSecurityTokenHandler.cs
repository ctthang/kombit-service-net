#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using Kombit.Samples.Common;

#endregion

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     A custom certificate security token handler which will do custom authentication and validation process
    /// </summary>
    public class CustomCertificateSecurityTokenHandler : X509SecurityTokenHandler
    {
        /// <summary>
        ///     Validate a certificate security token. Put custom validation here.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException("token");
            }
            //Below is a sample of code to illustrate how to add a custom validation for a X509 security token. 
            var clientCert = ((X509SecurityToken) token).Certificate;
            Logging.Instance.Debug(String.Format("Client certificate thumbprint: {0}", clientCert.Thumbprint));

            //Validate if a client certificate is a valid one in local store (in this sample, it will just simply compare with a predefined certificate
            if ((string.IsNullOrEmpty(clientCert.Thumbprint)) ||
                ((!clientCert.Thumbprint.Equals(Constants.AValidClientCertificate.Thumbprint))
                 && (!clientCert.Thumbprint.Equals(Constants.AValidOnBehalfOfCertificate.Thumbprint))))
            {
                Logging.Instance.Error("Invalid client credentials");
                throw new InvalidOperationException("Invalid client credentials. Expected " +
                                    Constants.AValidClientCertificate.Thumbprint.Trim(new[] {' '}) + " but received " +
                                    clientCert.Thumbprint.Trim(new[] {' '}));
            }
            var validatedClaims = base.ValidateToken(token);
            IList<Claim> claimsList = validatedClaims.SelectMany(validatedClaim => validatedClaim.Claims).ToList();

            //Remove duplicated nameIdentifier claims if there is
            if (claimsList.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                var nameIdentifierClaim = claimsList.First(c => c.Type == ClaimTypes.NameIdentifier);
                if (nameIdentifierClaim != null)
                    claimsList.Remove(nameIdentifierClaim);
            }
            //Below code is to fake nameIdentifier value
            var subjectNameId = Constants.GetSubjectNameId(clientCert);
            var newNameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, subjectNameId);
            newNameIdentifierClaim.Properties.Add(ClaimProperties.SamlNameIdentifierFormat, "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName");
            claimsList.Add(newNameIdentifierClaim);

            //Add a claim to indicate who executes this issue request
            claimsList.Add(new Claim(Constants.RequestorCertificateClaimType, clientCert.Thumbprint));

            var claimsIdentity = new ClaimsIdentity(claimsList,
                AuthenticationTypes.X509, ClaimTypes.Name, ClaimTypes.Role);
            return new ReadOnlyCollection<ClaimsIdentity>(new ClaimsIdentity[] {claimsIdentity});
        }
    }
}