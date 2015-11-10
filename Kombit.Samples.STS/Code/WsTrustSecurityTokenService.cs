using System;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.Xml;
using Kombit.Samples.Common;

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     A class that defines attributes and methods of security token service
    /// </summary>
    public class WsTrustSecurityTokenService : SecurityTokenService
    {
        private RequestSecurityToken clientRequest;

        /// <summary>
        ///     Initializes a new instance of the WsTrustSecurityTokenService class by a securityTokenServiceConfiguration
        /// </summary>
        /// <param name="securityTokenServiceConfiguration"></param>
        public WsTrustSecurityTokenService(SecurityTokenServiceConfiguration securityTokenServiceConfiguration)
            : base(securityTokenServiceConfiguration)
        {
        }

        /// <summary>
        ///     Gets a Scope object that contains information about the relying party (RP) associated with the specified request
        ///     (RST).
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
        {
            Logging.Instance.Debug("request.AppliesTo = " + request.AppliesTo);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.CommonRuntimeError,
                KombitErrorMessages.CommonRuntimeError);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.ConnectionResolutionError,
                KombitErrorMessages.ConnectionResolutionError);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.MalformedRequestError,
                KombitErrorMessages.MalformedRequestError);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.PathResolutionError,
                KombitErrorMessages.PathResolutionError);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.AuditUserRequestError,
                KombitErrorMessages.AuditUserRequestError);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.NotSupportedException,
                KombitErrorMessages.NotSupportedException);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.ConfigurationError,
                KombitErrorMessages.ConfigurationError);
            CheckIfErrorShouldBeThrown(request.AppliesTo, KombitEventIds.DatabaseError,
                KombitErrorMessages.DatabaseError);

            var scope = new Scope(request.AppliesTo.Uri.AbsoluteUri, Constants.SigningCredentials);
            scope.TokenEncryptionRequired = false;
            scope.SymmetricKeyEncryptionRequired = false;
            clientRequest = request;

            if (request.SecondaryParameters != null)
            {
                if (!string.IsNullOrEmpty(request.SecondaryParameters.KeyType))
                    request.KeyType = request.SecondaryParameters.KeyType;
                if (!string.IsNullOrEmpty(request.SecondaryParameters.TokenType))
                    request.TokenType = request.SecondaryParameters.TokenType;
            }

            return scope;
        }

        /// <summary>
        ///     Gets the appropriate security token handler for issuing a security token of the specified type.
        ///     A custom securityTokenHandler can be injected here
        /// </summary>
        /// <param name="requestedTokenType"></param>
        /// <returns></returns>
        protected override SecurityTokenHandler GetSecurityTokenHandler(string requestedTokenType)
        {
            return new CustomSaml2SecurityTokenHandler(clientRequest);
        }

        /// <summary>
        ///     This method is to check if an error should be thrown and sent back to client. It is to imimate some predefined
        ///     error types
        /// </summary>
        /// <param name="appliesTo"></param>
        /// <param name="eventId"></param>
        /// <param name="message"></param>
        private static void CheckIfErrorShouldBeThrown(EndpointReference appliesTo, int eventId, string message)
        {
            if (
                !appliesTo.Uri.AbsoluteUri.Trim(new char[] {'/'})
                    .Equals(Constants.DummyAppliesToError + eventId, StringComparison.InvariantCultureIgnoreCase))
                return;

            var faultMessage = new StsFaultDetail() {Message = string.Format(Constants.DummyErrorMessagePattern, message) };
            throw new FaultException<StsFaultDetail>(faultMessage, new FaultReason(string.Format(Constants.DummyDetailErrorMessagePattern, message)), new FaultCode(Convert.ToString(eventId)));
        }

        /// <summary>
        ///     This method returns a collection of output subjects to be included in the issued token.
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="request"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal,
            RequestSecurityToken request, Scope scope)
        {
            var requestorClaim = principal.Claims.FirstOrDefault(c => c.Type == Constants.RequestorCertificateClaimType);
            if (requestorClaim == null)
            {
                Logging.Instance.Error(
                    "Missing requestor claim, please make sure that the request is using certificate authentication");
                throw new ArgumentException(
                    "Missing requestor claim, please make sure that the request is using certificate authentication");
            }
            var claimsIdentity =
                new ClaimsIdentity(AuthenticationTypes.X509, ClaimTypes.Name, ClaimTypes.Role);

            foreach (var claim in principal.Claims)
            {
                if (claim.Type == ClaimTypes.NameIdentifier)
                    claimsIdentity.AddClaim(claim);
            }

            claimsIdentity.AddClaim(
                new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationinstant",
                    XmlConvert.ToString(DateTime.UtcNow, "yyyy-MM-ddTHH:mm:ss.fffZ"),
                    "http://www.w3.org/2001/XMLSchema#dateTime"));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.X509));

            //Requestor name = Hans Jensen
            //OnBehalfOf username = Maren Jensdatter
            if (request.OnBehalfOf != null)
            {
                SecurityToken onBehalfOfToken = null;
                Saml2SecurityToken obTokenAsNormal = null;
                X509SecurityToken obTokenAsProxy = null;
                try
                {
                    onBehalfOfToken = request.OnBehalfOf.GetSecurityToken();
                    obTokenAsNormal = onBehalfOfToken as Saml2SecurityToken;
                    obTokenAsProxy = onBehalfOfToken as X509SecurityToken;

                    if ((obTokenAsNormal == null) && (obTokenAsProxy == null))
                    {
                        Logging.Instance.Error(
                            "cannot read onbehalfof token, please make sure that the onbehalfof element is saml2SecurityToken or x509securityToken");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Instance.Error("Cannot read the onbehalfof token" + ex);
                    throw new FaultException("Cannot read the onbehalfof token" + ex);
                }
                //Should issue claims which belongs to the user on OBO element. Following are fake claims for OBO user.
                claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:AssuranceLevel", "4"));
                claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:SpecVer", "DK-SAML-2.0"));
                claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:KombitSpecVer", "1.0"));
                claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:CvrNumberIdentifier", "34051178-FID:54846685"));
            }
            else
            {
                if (requestorClaim.Value == Constants.AValidClientCertificate.Thumbprint)
                {
                    //This issue request is come from the caller, following claims are fake for caller user
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:AssuranceLevel", "1"));
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:SpecVer", "DK-SAML-2.0"));
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:KombitSpecVer", "1.0"));
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:CvrNumberIdentifier",
                        "34051178-FID:54846685"));
                }
                if (requestorClaim.Value == Constants.AValidOnBehalfOfCertificate.Thumbprint)
                {
                    //This issue request is come from the OBO user, following claims are fake for OBO user
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:AssuranceLevel", "4"));
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:SpecVer", "DK-SAML-2.0"));
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:KombitSpecVer", "1.0"));
                    claimsIdentity.AddClaim(new Claim("dk:gov:saml:attribute:CvrNumberIdentifier",
                        "34051178-FID:54846685"));
                }
            }
            var bppClaim = GenerateBppClaim();
            claimsIdentity.AddClaim(bppClaim);
            return claimsIdentity;
        }

        /// <summary>
        ///     This method is to generate bpp claim value. In this sample, it is actually loaded from app settings.
        /// </summary>
        /// <returns></returns>
        private Claim GenerateBppClaim()
        {
            return new Claim("dk:gov:saml:attribute:Privileges_intermediate", Constants.BppValue);
        }
    }
}