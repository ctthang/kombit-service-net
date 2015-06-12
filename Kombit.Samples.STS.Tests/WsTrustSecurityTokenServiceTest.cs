using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using Kombit.Samples.Common;
using Kombit.Samples.STS.Code;
using Ploeh.AutoFixture;
using Xunit;

namespace Kombit.Samples.STS.Tests
{
    /// <summary>
    ///     A collection of method that tests against WSTrustSecurityTokenService
    /// </summary>
    public class WsTrustSecurityTokenServiceTest
    {
        /// <summary>
        ///     This method is to make sure GetScope runs correctly
        /// </summary>
        [Fact]
        public void GetScopeCorrectly()
        {
            var fixture = new StsFixture();
            var service = new WsTrustSecurityTokenServiceWrapper(new WsTrustServiceConfiguration());
            var claimPrincipal = fixture.Create<ClaimsPrincipal>();
            var appliesTo = fixture.Create<Uri>().AbsoluteUri;
            var rst = new RequestSecurityToken()
            {
                AppliesTo = new EndpointReference(appliesTo),
                RequestType = RequestTypes.Issue,
                TokenType = Common.WSTrust13.Constants.TokenTypes.OasisWssSaml2TokenProfile11
            };

            var scope = service.CallGetScope(claimPrincipal, rst);
            Assert.True(scope.EncryptingCredentials == null);
            Assert.True(scope.SymmetricKeyEncryptionRequired == false);
            Assert.True(scope.TokenEncryptionRequired == false);
            Assert.True(scope.AppliesToAddress == appliesTo);
        }

        /// <summary>
        ///     This method is to make sure GetOutputClaimsIdentity runs correctly
        /// </summary>
        [Fact]
        public void GetOutputClaimsIdentityCorrectly()
        {
            var fixture = new StsFixture();
            var service = new WsTrustSecurityTokenServiceWrapper(new WsTrustServiceConfiguration());
            var claimPrincipal = fixture.Create<ClaimsPrincipal>();
            claimPrincipal.AddIdentity(
                new ClaimsIdentity(new List<Claim>
                {
                    new Claim(Constants.RequestorCertificateClaimType, Constants.AValidClientCertificate.Thumbprint)
                }));
            var appliesTo = fixture.Create<Uri>().AbsoluteUri;
            var rst = new RequestSecurityToken()
            {
                AppliesTo = new EndpointReference(appliesTo),
                RequestType = RequestTypes.Issue,
                TokenType = Common.WSTrust13.Constants.TokenTypes.OasisWssSaml2TokenProfile11
            };

            var outputClaimsIdentity = service.CallGetOutputClaimsIdentity(claimPrincipal, rst);
            Assert.True(outputClaimsIdentity != null);
            Assert.True(outputClaimsIdentity.Claims != null);
            Assert.Equal(outputClaimsIdentity.Claims.Count(), 7);
            Assert.True(outputClaimsIdentity.AuthenticationType == AuthenticationTypes.X509);
            Assert.True(outputClaimsIdentity.NameClaimType == ClaimTypes.Name);
            Assert.True(outputClaimsIdentity.RoleClaimType == ClaimTypes.Role);
            var bppClaim =
                outputClaimsIdentity.Claims.FirstOrDefault(
                    x => x.Type == "dk:gov:saml:attribute:Privileges_intermediate");
            Assert.True(bppClaim != null);
            Assert.True(bppClaim.Value == Constants.BppValue);
        }

        /// <summary>
        ///     This method is to make sure that STS will throw appropriate error for each request
        /// </summary>
        [Fact]
        public void ShowAppropriateErrorWhenIssueToken()
        {
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.CommonRuntimeError,
                KombitErrorMessages.CommonRuntimeError);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.ConnectionResolutionError,
                KombitErrorMessages.ConnectionResolutionError);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.MalformedRequestError,
                KombitErrorMessages.MalformedRequestError);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.PathResolutionError,
                KombitErrorMessages.PathResolutionError);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.AuditUserRequestError,
                KombitErrorMessages.AuditUserRequestError);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.NotSupportedException,
                KombitErrorMessages.NotSupportedException);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError,
                KombitEventIds.ConfigurationError,
                KombitErrorMessages.ConfigurationError);
            ThrowExceptionIfAppliesToIsADummyErrorAddress(Constants.DummyAppliesToError, KombitEventIds.DatabaseError,
                KombitErrorMessages.DatabaseError);
        }

        /// <summary>
        ///     This method is to check if the exception thrown for each request is correct or not
        /// </summary>
        /// <param name="appliesTo"></param>
        /// <param name="expectedEventId"></param>
        /// <param name="expectedMessage"></param>
        private void ThrowExceptionIfAppliesToIsADummyErrorAddress(string appliesTo, int expectedEventId,
            string expectedMessage)
        {
            var fixture = new StsFixture();
            var service = new WsTrustSecurityTokenServiceWrapper(new WsTrustServiceConfiguration());
            var claimPrincipal = fixture.Create<ClaimsPrincipal>();
            var rst = new RequestSecurityToken()
            {
                AppliesTo = new EndpointReference(appliesTo + expectedEventId),
                RequestType = RequestTypes.Issue,
                TokenType = Common.WSTrust13.Constants.TokenTypes.OasisWssSaml2TokenProfile11
            };
            try
            {
                service.CallGetScope(claimPrincipal, rst);
            }
            catch (FaultException ex)
            {
                Assert.True(ex.Message.IndexOf(expectedMessage, StringComparison.Ordinal) > 0);
            }
        }
    }

    /// <summary>
    ///     A wrapper of STS service
    /// </summary>
    internal class WsTrustSecurityTokenServiceWrapper : WsTrustSecurityTokenService
    {
        public WsTrustSecurityTokenServiceWrapper(WsTrustServiceConfiguration config)
            : base(config)
        {
        }

        internal Scope CallGetScope(ClaimsPrincipal principal, RequestSecurityToken request)
        {
            return GetScope(principal, request);
        }

        internal ClaimsIdentity CallGetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken rst)
        {
            return GetOutputClaimsIdentity(principal, rst, null);
        }
    }
}