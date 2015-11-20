using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml;
using Kombit.Samples.BasicPrivilegeProfileParser;
using Kombit.Samples.Common;
using Kombit.Samples.Common.CustomHeaders;
using Kombit.Samples.Common.Service.MessageContracts;
using Kombit.Samples.Consumer.Sts;
using Xunit;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     A collection of methods that does various integration tests to STS and Service service
    /// </summary>
    public class Consumer
    {
        /// <summary>
        ///     A test to make sure that STS will return correct error and those are handled correctly on client side
        /// </summary>
        [Fact]
        public void HandleCommonErrorsWhenIssueToken()
        {
            SendIssueRequestWhichWillReceiveConnectionNotFoundError();
            SendIssueRequestWhichWillBeFailedBecauseOfAuthentication();
            SendIssueRequestWhichWillBeFailedBecauseOfInvaidRst();
        }

        /// <summary>
        /// A test to demonstrate how client handles common errors such as STS cannot find any connection corresponding to the request
        /// </summary>
        public void SendIssueRequestWhichWillReceiveConnectionNotFoundError()
        {
            var isErrorThrown = false;
            try
            {
                ConnectionHelper.SendRequestSecurityTokenRequest("https://consumer.kombit.dk/noconnectionfound", null,
                    Constants.ClientCertificate, null);
            }
            catch (FaultException e)
            {
                isErrorThrown = true;
                Assert.Equal(e.Code.Name, "5002");
                Assert.True(e.Message.Contains("Path resolution: no connection"));
                Assert.True(e.Message.IndexOf("https://consumer.kombit.dk/noconnectionfound", StringComparison.Ordinal) > 0);
            }

            Assert.True(isErrorThrown);
        }

        /// <summary>
        /// A test to demonstrate how client handles common errors such as authentication check is failed
        /// </summary>
        public void SendIssueRequestWhichWillBeFailedBecauseOfAuthentication()
        {
            var isErrorThrown = false;
            try
            {
                ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null,
                    Constants.StsServiceCertificate, null);
            }
            catch (FaultException e)
            {
                isErrorThrown = true;
                Assert.Equal(e.Code.Name, "5607");
                Assert.True(e.Message.Contains("No mapped user exists for thumbprint"));
                Assert.True(
                    e.Message.IndexOf(Constants.StsServiceCertificate.Thumbprint, StringComparison.Ordinal) > 0);
            }

            Assert.True(isErrorThrown);
        }

        /// <summary>
        /// A test to demonstrate how client handles common errors such as a Rst contains an invalid CVR
        /// </summary>
        public void SendIssueRequestWhichWillBeFailedBecauseOfInvaidRst()
        {
            var isErrorThrown = false;
            try
            {
                ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null,
                    Constants.ClientCertificate, string.Empty, null);
            }
            catch (FaultException e)
            {
                isErrorThrown = true;
                Assert.Equal(e.Code.Name, "5015");
                Assert.True(e.Message.Contains("It is expected that the received STR must contain a CVR claim. Please check to make sure it includes the CVR claim."));
            }

            Assert.True(isErrorThrown);
        }

        /// <summary>
        ///     A test to make sure that the metadata endpoint is enabled
        /// </summary>
        [Fact]
        public void CanRetrieveMetadataFromMexEndpoint()
        {
            var metaTransfer
                = new MetadataExchangeClient(Constants.StsMexEndpointUri,
                    MetadataExchangeClientMode.HttpGet) {ResolveMetadataReferences = true};
            MetadataSet otherDocs = metaTransfer.GetMetadata();
            Assert.True(otherDocs != null);
        }

        /// <summary>
        ///     An integration test to make sure that client can send a RST to negotiate a security token from STS and use it to
        ///     accesss remove service service
        /// </summary>
        [Fact]
        public void SendRstAndThenExecuteServiceServiceSuccessfully()
        {
            //Send request to issue security token from Samples.STS
            var token = ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri,
                null, Constants.ClientCertificate, null);

            ValidateIssuedToken(token, Constants.ServiceAddressUri.AbsoluteUri,
                new List<Saml2Attribute>()
                {
                    new Saml2Attribute("dk:gov:saml:attribute:SpecVer", "DK-SAML-2.0")
                });

            //Use the issued token to execute the server on Samples.Service
            var channel = ConnectionHelper.GenerateServiceChannel(token);
            var result =
                channel.Ping(new Ping()
                {
                    Framework = new LibertyFrameworkHeader(),
                    MessageToPing = "Ping to service ...."
                });

            var bppGroupList = PrivilegeGroupParser.Parse(Constants.BppValue);

            //Verify the response from user context service
            Assert.True(bppGroupList != null);
            Assert.True(!string.IsNullOrEmpty(result.MessageToResponse));
            Assert.True(result.MessageToResponse.Contains("MessageId:"));
            Assert.True(result.MessageToResponse.IndexOf("Bpp:", StringComparison.InvariantCultureIgnoreCase) > 13);
            //Assert.True(result.MessageToResponse.Contains(PrivilegeGroupParser.ToJsonString(bppGroupList)));
        }

        /// <summary>
        ///     A test to show up how to handle Error response in case of sending a RST with wrong format
        /// </summary>
        [Fact]
        public void SendRstWithWrongFormatWillReturnErrorResponse()
        {
            //Send request to issue security token from Samples.STS
            Assert.Throws<FaultException>(
                () =>
                    ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null,
                        Constants.ClientCertificate, ModifyMessageBody));
        }

        /// <summary>
        ///     A test to show up how to handle Error response in case of sending a RST with wrong signature
        /// </summary>
        [Fact]
        public void SendRstWithInvalidSignatureWillReturnErrorResponse()
        {
            //Send request to issue security token from Samples.STS
            Assert.Throws<MessageSecurityException>(
                () =>
                    ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null,
                        Constants.ClientCertificate, null,
                        () =>
                            new CustomTextMessageBindingElement(
                                messageEncoderFactory => new CustomTextMessageEncoderFactory(messageEncoderFactory,
                                    MessageModifier.TamperMessage))));
        }

        /// <summary>
        ///     This test case is used in conjunction with the ReplayRstWillReturnErrorResponse test case to prove that the
        ///     ReplayTextMessageEncoder really works
        /// </summary>
        [Fact]
        public void SendRstTwiceWithDifferentMessagesWillBeAccepted()
        {
            //Send request to issue security token from Samples.STS
            var replayMessageModifier = new ReplayMessageModifier(false);
            var customTextMessageBindingElement =
                new CustomTextMessageBindingElement(
                    messageEncoderFactory => new CustomTextMessageEncoderFactory(messageEncoderFactory,
                        replayMessageModifier.Modify));
            ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null, 
                Constants.ClientCertificate, null, () => customTextMessageBindingElement);
            ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null, 
                Constants.ClientCertificate, null, () => customTextMessageBindingElement);
        }

        /// <summary>
        ///     This test case sends the same RST twice to simulate replay attack
        /// </summary>
        [Fact]
        public void ReplayRstWillReturnErrorResponse()
        {
            //Send request to issue security token from Samples.STS
            Assert.Throws<MessageSecurityException>(() =>
            {
                var replayMessageModifier = new ReplayMessageModifier(true);
                var customTextMessageBindingElement =
                    new CustomTextMessageBindingElement(
                        messageEncoderFactory => new CustomTextMessageEncoderFactory(messageEncoderFactory,
                            replayMessageModifier.Modify));
                ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null,
                    Constants.ClientCertificate, null, () => customTextMessageBindingElement);
                ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null, 
                    Constants.ClientCertificate, null, () => customTextMessageBindingElement);
            });
        }

        /// <summary>
        ///     This test case sends an RST with valid timestamp to the STS. This is used in conjunction with the
        ///     SendExpiredRstWillReturnErrorResponse test case to prove that the other test works.
        /// </summary>
        [Fact]
        public void SendRstWithValidTimestampWillNotThrowError()
        {
            //Send request to issue security token from Samples.STS
            var customTextMessageBindingElement =
                new CustomTextMessageBindingElement(
                    messageEncoderFactory => new CustomTextMessageEncoderFactory(messageEncoderFactory,
                        MessageModifier.DelaySending));
            ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null, 
                Constants.ClientCertificate, null, () => customTextMessageBindingElement, 3);
            // Clockskew is 8 seconds, sleep 10 second -> duration = 3 second: still valid
        }

        /// <summary>
        ///     This test case sends an RST with invalid timestamp to the STS
        /// </summary>
        [Fact]
        public void SendExpiredRstWillReturnErrorResponse()
        {
            //Send request to issue security token from Samples.STS
            Assert.Throws<MessageSecurityException>(() =>
            {
                var customTextMessageBindingElement =
                    new CustomTextMessageBindingElement(
                        messageEncoderFactory => new CustomTextMessageEncoderFactory(messageEncoderFactory,
                            MessageModifier.DelaySending));
                ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri, null,
                    Constants.ClientCertificate, null, () => customTextMessageBindingElement, 1);
                // Clockskew is 8 seconds, sleep 10 second -> duration = 1 second: invalid
            });
        }

        /// <summary>
        ///     An integration test to show up how to negotidate a security token with proxy onbehalfof
        /// </summary>
        [Fact]
        public void SendRstWithProxyOnBehalfOfSuccessfully()
        {
            var originalTokenElement =
                new SecurityTokenElement(new X509SecurityToken(Constants.AValidOnBehalfOfCertificate));
            var token = ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri,
                originalTokenElement, 
                Constants.ClientCertificate, null);

            //Verify the issued token responsed from STS service
            Assert.True(token != null);
            Assert.True(DateTime.UtcNow.Subtract(token.ValidFrom).TotalMinutes < 1);
            ValidateIssuedToken(token, Constants.ServiceAddressUri.AbsoluteUri,
                new List<Saml2Attribute>()
                {
                    new Saml2Attribute("dk:gov:saml:attribute:SpecVer", "DK-SAML-2.0")
                });
        }

        /// <summary>
        ///     An integration test to show up how to negotidate a security token with normal onbehalfof
        /// </summary>
        [Fact]
        public void SendRstWithNormalOnBehalfOfSuccessfully()
        {
            var originalToken = GenerateBootstrapToken();
            //Send request to issue security token from Samples.STS
            var token = ConnectionHelper.SendRequestSecurityTokenRequest(Constants.ServiceAddressUri.AbsoluteUri,
                new SecurityTokenElement(originalToken), 
                Constants.ClientCertificate, null);

            ValidateIssuedToken(token, Constants.ServiceAddressUri.AbsoluteUri,
                new List<Saml2Attribute>()
                {
                    new Saml2Attribute("dk:gov:saml:attribute:SpecVer", "DK-SAML-2.0")
                });
        }

        /// <summary>
        ///     a helper to valdate if an issued token is valid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="appliesTo"></param>
        /// <param name="mustHaveAttribute"></param>
        private static void ValidateIssuedToken(SecurityToken token, string appliesTo,
            IList<Saml2Attribute> mustHaveAttribute)
        {
            //Verify the issued token responsed from STS service
            Assert.True(token != null);
            Assert.True(DateTime.UtcNow.Subtract(token.ValidFrom).TotalMinutes < 1);

            var genericToken = token as GenericXmlSecurityToken;
            Assert.True(genericToken != null);

            var handlers = SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection();

            var reader = new XmlTextReader(new StringReader(genericToken.TokenXml.OuterXml));
            var samlSecurityToken = handlers.ReadToken(reader);

            var saml2Token = samlSecurityToken as Saml2SecurityToken;
            Assert.True(saml2Token != null);
            var attributeStatement = saml2Token.Assertion.Statements[0] as Saml2AttributeStatement ??
                                     saml2Token.Assertion.Statements[1] as Saml2AttributeStatement;
            Assert.True(attributeStatement != null);
            foreach (var saml2Attribute in mustHaveAttribute)
            {
                Assert.True(AttributeExistsInCollection(attributeStatement.Attributes, saml2Attribute));
            }
            Assert.True(attributeStatement.Attributes[0].NameFormat.AbsoluteUri ==
                        "urn:oasis:names:tc:SAML:2.0:attrname-format:basic");
            Assert.True(saml2Token.Assertion.Subject.NameId != null);
            Assert.True(saml2Token.Assertion.Subject.NameId.Format.AbsoluteUri ==
                        "urn:oasis:names:tc:SAML:1.1:nameid-format:X509SubjectName");
            Assert.True(saml2Token.Assertion.Subject.SubjectConfirmations.Count > 0);
            Assert.True(saml2Token.Assertion.Subject.SubjectConfirmations[0].SubjectConfirmationData != null);
            Assert.True(
                saml2Token.Assertion.Subject.SubjectConfirmations[0].SubjectConfirmationData.Recipient.AbsoluteUri ==
                appliesTo);
        }

        /// <summary>
        ///     A helper method to make sure that an attribute exist in responsed assertion
        /// </summary>
        /// <param name="attributes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool AttributeExistsInCollection(IEnumerable<Saml2Attribute> attributes, Saml2Attribute value)
        {
            return
                attributes.Any(
                    saml2Attribute =>
                        (saml2Attribute.Name == value.Name) &&
                        (saml2Attribute.Values[0].Trim(new char[] {'/'}) == value.Values[0].Trim(new char[] {'/'})));
        }

        /// <summary>
        ///     This method is to generate a dummy saml2 security token to be used in OBO element
        /// </summary>
        /// <returns></returns>
        private SecurityToken GenerateBootstrapToken()
        {
            return ConnectionHelper.SendRequestSecurityTokenRequest(Constants.BootstraptokenServiceBaseAddress, null,
                Constants.AValidOnBehalfOfCertificate, null);
        }

        /// <summary>
        ///     Modify the *body* of a message to make it's format invalid.
        /// </summary>
        /// <param name="oldMessage"></param>
        /// <returns></returns>
        private Message ModifyMessageBody(Message oldMessage)
        {
            //load the old message into XML
            MessageBuffer msgbuf = oldMessage.CreateBufferedCopy(int.MaxValue);
            Message tmpMessage = msgbuf.CreateMessage();
            XmlDictionaryReader xdr = tmpMessage.GetReaderAtBodyContents();
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(xdr);
            xdr.Close();

            //transform the xmldocument
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            nsmgr.AddNamespace("trust", "http://docs.oasis-open.org/ws-sx/ws-trust/200512");

            XmlNode node = xdoc.SelectSingleNode("//trust:RequestSecurityToken", nsmgr);
            node.RemoveAll();
            MemoryStream ms = new MemoryStream();
            XmlWriter xw = XmlWriter.Create(ms);
            xdoc.Save(xw);
            xw.Flush();
            xw.Close();
            ms.Position = 0;
            XmlReader xr = XmlReader.Create(ms);

            //create new message from modified XML document
            Message newMessage = Message.CreateMessage(oldMessage.Version, null, xr);
            newMessage.Headers.CopyHeadersFrom(oldMessage);
            newMessage.Properties.CopyProperties(oldMessage.Properties);
            return newMessage;
        }
    }
}