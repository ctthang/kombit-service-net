#region

using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

#endregion

namespace Kombit.Samples.Service.Code
{
    /// <summary>
    ///     To log soap message sent and received from Service service
    /// </summary>
    public class LogMessageDispatchMessageInspector : IDispatchMessageInspector
    {
        private readonly string _receivedSoapMessageLogFile = @"c:\Temp\";

        /// <summary>
        ///     Initializes a new instance of the LogMessageDispatchMessageInspector class
        /// </summary>
        public LogMessageDispatchMessageInspector()
        {
            if (ConfigurationManager.AppSettings["SoapMessageLogLocation"] != null)
                _receivedSoapMessageLogFile = ConfigurationManager.AppSettings["SoapMessageLogLocation"];
        }

        /// <summary>
        ///     This method is to log soap message after receiving request from client side
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <param name="instanceContext"></param>
        /// <returns></returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            File.WriteAllText(_receivedSoapMessageLogFile + "Kombit.Service.ReceivedRequest.xml", request.ToString());
            return null;
        }

        /// <summary>
        ///     This method is to log soap message before sending reply to client side
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            File.WriteAllText(_receivedSoapMessageLogFile + "Kombit.Service.SentResponse.xml", reply.ToString());
        }
    }

    /// <summary>
    ///     A behaviour which is hook to endpoint's behaviours to register log message handler
    /// </summary>
    public class HookLogMessageBehavior : IEndpointBehavior
    {
        /// <summary>
        ///     Validate service endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        /// <summary>
        ///     Add binding parameters
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="bindingParameters"></param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        ///     Apply dispatch behaviour
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="endpointDispatcher"></param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new LogMessageDispatchMessageInspector());
        }

        /// <summary>
        ///     Apply client behaviour
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="clientRuntime"></param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }
    }
}