using System.IdentityModel.Tokens;
using Kombit.Samples.STS.Code;
using Xunit;

namespace Kombit.Samples.STS.Tests
{
    /// <summary>
    ///     A collection of method that tests against WsTrustServiceConfiguration
    /// </summary>
    public class WsTrustServiceConfigurationTest
    {
        /// <summary>
        ///     This method is to make sure that the configuration is setup correctly
        /// </summary>
        [Fact]
        public void ConfigurationIsSetupCorrectly()
        {
            var config = new WsTrustServiceConfiguration();
            Assert.True(config.SecurityTokenHandlers != null);
            Assert.True(config.SecurityTokenHandlers.Count > 2);
            Assert.True(config.SigningCredentials != null);
            Assert.True((config.SigningCredentials as X509SigningCredentials) != null);
            Assert.True((config.SigningCredentials as X509SigningCredentials).Certificate.Thumbprint ==
                        Constants.SigningCertificate.Thumbprint);
            Assert.True(config.TokenIssuerName == Constants.BaseAddress);
            Assert.True(config.DefaultTokenType == Common.WSTrust13.Constants.TokenTypes.OasisWssSaml2TokenProfile11);
        }
    }
}