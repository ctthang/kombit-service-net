using System;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;
using System.Xml;
using Kombit.Samples.BasicPrivilegeProfileParser;
using Kombit.Samples.Common.Binding.Data;
using Kombit.Samples.Common.CustomHeaders;
using Kombit.Samples.Common.Service.MessageContracts;
using Kombit.Samples.Common.Service.ServiceInterfaces;

namespace Kombit.Samples.Service.Code
{
    /// <summary>
    /// Service provider which is secured by security token issued by STS stub
    /// </summary>
    public class Service : IService
    {
        /// <summary>
        /// Ping to the service which will return a message generated from pingRequest and some pice of information extracted from current identity
        /// </summary>
        /// <param name="pingRequest"></param>
        /// <returns></returns>
        public PingResponse Ping(Ping pingRequest)
        {
            ValidateLibertyFrameworkHeader(pingRequest.Framework);

            string businessResponse = ProcessBusinessLogic(pingRequest.MessageToPing);

            var pingReply = new PingResponse();
            pingReply.MessageToResponse = businessResponse;
            pingReply.Framework = new LibertyFrameworkHeader();

            InsertWsAddressingMessageIdOnResponse();

            return pingReply;
        }
        /// <summary>
        /// Validate whehter there is a valid liberty framework element in header message
        /// </summary>
        /// <param name="framework"></param>
        private void ValidateLibertyFrameworkHeader(LibertyFrameworkHeader framework)
        {
            if (framework == null)
            {
                throw new FaultException<FrameworkFault>(null, new FaultReason("Missing frameworkheader"),
                    new FaultCode("FrameworkVersionMismatch", "urn:liberty:sb:2006-08"));
            }
            if (framework.Profile != "urn:liberty:sb:profile:basic")
            {
                throw new FaultException<FrameworkFault>(null, new FaultReason("Wrong profile"),
                    new FaultCode("FrameworkVersionMismatch", "urn:liberty:sb:2006-08"));
            }
        }
        /// <summary>
        /// Generate message to response to client
        /// </summary>
        /// <param name="messageToPing"></param>
        /// <returns></returns>
        private string ProcessBusinessLogic(string messageToPing)
        {
            string result;
            var identity = (ClaimsIdentity) (OperationContext.Current.ClaimsPrincipal.Identity);
            var bpp =
                identity.Claims.FirstOrDefault(claim => claim.Type == "dk:gov:saml:attribute:Privileges_intermediate");
            if (bpp == null)
                result = Constants.ResponseMessage;
            else
            {
                var bppGroupList = PrivilegeGroupParser.Parse(bpp.Value);
                UniqueId messageId = OperationContext.Current.RequestContext.RequestMessage.Headers.MessageId;
                result = string.Format("MessageId: {0}.\nBpp: {1}", messageId,
                    PrivilegeGroupParser.ToJsonString(bppGroupList));
            }

            var str = messageToPing + result;
            return str;
        }
        /// <summary>
        /// Insert a message id in to response
        /// </summary>
        private void InsertWsAddressingMessageIdOnResponse()
        {
            OperationContext.Current.OutgoingMessageHeaders.MessageId = new System.Xml.UniqueId(new Guid());
        }
    }
}