using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     To log soap message sent and received from service service
    /// </summary>
    public class LogServiceMessageClientMessageInspector : IClientMessageInspector
    {
        private readonly string _receivedSoapMessageLogFile = @"c:\Temp\";

        /// <summary>
        ///     Initializes a new instance of the LogServiceMessageClientMessageInspector class
        /// </summary>
        public LogServiceMessageClientMessageInspector()
        {
            if (ConfigurationManager.AppSettings["SoapMessageLogLocation"] != null)
                _receivedSoapMessageLogFile = ConfigurationManager.AppSettings["SoapMessageLogLocation"];
        }

        /// <summary>
        ///     This method is to log soap message before sending request to service
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            //var path = string.Format("{0}Kombit.Consumer.SentRequestToService_{1}.txt", _receivedSoapMessageLogFile,
            //    DateTime.Now.ToString("hh_mm_ss.fff"));
            var path = string.Format("{0}kombit.Consumer.SentRequestToService.xml", _receivedSoapMessageLogFile);
            File.WriteAllText(path, request.ToString());
            return null;
        }

        /// <summary>
        ///     This method is to log soap message after receiving response from service
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //var path = string.Format("{0}Kombit.Consumer.ReceivedResponseFromService_{1}.txt", _receivedSoapMessageLogFile,
            //    DateTime.Now.ToString("hh_mm_ss.fff"));
            var path = string.Format("{0}Kombit.Consumer.ReceivedResponseFromService.xml",
                _receivedSoapMessageLogFile);
            File.WriteAllText(path, reply.ToString());
        }
    }

    /// <summary>
    ///     A behaviour which is hook to endpoint's behaviours to register log message handler
    /// </summary>
    public class HookLogServiceMessageBehavior : IEndpointBehavior
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
        }

        /// <summary>
        ///     Apply client behaviour
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="clientRuntime"></param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new LogServiceMessageClientMessageInspector());
        }
    }
}