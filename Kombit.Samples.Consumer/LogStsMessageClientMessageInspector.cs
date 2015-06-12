using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Kombit.Samples.Consumer
{
    /// <summary>
    ///     To log soap message sent and received from STS service
    /// </summary>
    public class LogStsMessageClientMessageInspector : IClientMessageInspector
    {
        private readonly Func<Message, Message> messesagModifier;
        private readonly string receivedSoapMessageLogFile = @"c:\Temp\";

        /// <summary>
        ///     Initializes a new instance of the LogStsMessageClientMessageInspector class
        /// </summary>
        /// <param name="messesagModifier"></param>
        public LogStsMessageClientMessageInspector(Func<Message, Message> messesagModifier)
        {
            if (ConfigurationManager.AppSettings["SoapMessageLogLocation"] != null)
                receivedSoapMessageLogFile = ConfigurationManager.AppSettings["SoapMessageLogLocation"];

            this.messesagModifier = messesagModifier;
        }

        /// <summary>
        ///     This method is to log soap message before sending request to sts
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            //var path = string.Format("{0}Kombit.Consumer.SentRequestToSTS_{1}.txt", receivedSoapMessageLogFile,
            //    DateTime.Now.ToString("hh_mm_ss.fff"));
            var path = string.Format("{0}Kombit.Consumer.SentRequestToSTS.xml", receivedSoapMessageLogFile);
            File.WriteAllText(path, request.ToString());

            if (messesagModifier != null)
            {
                request = messesagModifier(request);
                //var path1 = string.Format("{0}Kombit.Consumer.SentRequestToSTS-modified_{1}.txt", receivedSoapMessageLogFile,
                //DateTime.Now.ToString("hh_mm_ss.fff"));
                var path1 = string.Format("{0}Kombit.Consumer.SentRequestToSTS-modified.xml",
                    receivedSoapMessageLogFile);
                File.WriteAllText(path1, request.ToString());
            }
            return null;
        }

        /// <summary>
        ///     This method is to log soap message after receiving response from sts
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            //var path = string.Format("{0}Kombit.Consumer.ReceivedResponseFromSTS_{1}.txt", receivedSoapMessageLogFile,
            //    DateTime.Now.ToString("hh_mm_ss.fff"));
            var path = string.Format("{0}Kombit.Consumer.ReceivedResponseFromSTS.xml", receivedSoapMessageLogFile);
            File.WriteAllText(path, reply.ToString());
        }
    }

    /// <summary>
    ///     A behaviour which is hook to endpoint's behaviours to register log message handler
    /// </summary>
    public class HookLogStsMessageBehavior : IEndpointBehavior
    {
        private readonly Func<Message, Message> messesagModifier;

        /// <summary>
        ///     Initializes a new instance of the HookLogStsMessageBehavior class
        /// </summary>
        /// <param name="messesagModifier"></param>
        public HookLogStsMessageBehavior(Func<Message, Message> messesagModifier)
        {
            this.messesagModifier = messesagModifier;
        }

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
            clientRuntime.MessageInspectors.Add(new LogStsMessageClientMessageInspector(messesagModifier));
        }
    }
}