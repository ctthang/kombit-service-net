#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Description;
using System.Xml;
using Message = System.Web.Services.Description.Message;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;

#endregion

namespace Kombit.Samples.STS.Code
{
    /// <summary>
    ///     Implements methods that can be used to represented in metadata endpoint for either a service or client application
    /// </summary>
    internal class DecoratedEndpointBehavior : IWsdlExportExtension, IEndpointBehavior
    {
        // Fields
        private readonly StringDictionary _unSupportedOperations = new StringDictionary();

        /// <summary>
        ///     Initializes a new instance of the DecoratedEndpointBehavior class
        /// </summary>
        public DecoratedEndpointBehavior()
        {
            _unSupportedOperations.Add("TrustFeb2005CancelAsync", null);
            _unSupportedOperations.Add("TrustFeb2005RenewAsync", null);
            _unSupportedOperations.Add("TrustFeb2005ValidateAsync", null);
            _unSupportedOperations.Add("TrustFeb2005CancelResponseAsync", null);
            _unSupportedOperations.Add("TrustFeb2005IssueResponseAsync", null);
            _unSupportedOperations.Add("TrustFeb2005RenewResponseAsync", null);
            _unSupportedOperations.Add("TrustFeb2005ValidateResponseAsync", null);
            _unSupportedOperations.Add("Trust13CancelAsync", null);
            _unSupportedOperations.Add("Trust13RenewAsync", null);
            _unSupportedOperations.Add("Trust13ValidateAsync", null);
            _unSupportedOperations.Add("Trust13CancelResponseAsync", null);
            _unSupportedOperations.Add("Trust13IssueResponseAsync", null);
            _unSupportedOperations.Add("Trust13RenewResponseAsync", null);
            _unSupportedOperations.Add("Trust13ValidateResponseAsync", null);
            _unSupportedOperations.Add("Trust13Cancel", null);
            _unSupportedOperations.Add("Trust13Renew", null);
            _unSupportedOperations.Add("Trust13Validate", null);
            _unSupportedOperations.Add("Trust13CancelResponse", null);
            _unSupportedOperations.Add("Trust13IssueResponse", null);
            _unSupportedOperations.Add("Trust13RenewResponse", null);
            _unSupportedOperations.Add("Trust13ValidateResponse", null);
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
        ///     This method is to apply dispatch behaviour if there is
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="endpointDispatcher"></param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        /// <summary>
        ///     This method is to apply client behaviour if there is
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="clientRuntime"></param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        ///     This method is to export contract if needed
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="context"></param>
        public void ExportContract(WsdlExporter exporter, WsdlContractConversionContext context)
        {
        }

        /// <summary>
        ///     This method is to export endpoint if needed
        /// </summary>
        /// <param name="exporter"></param>
        /// <param name="context"></param>
        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            ServiceDescription serviceDescription = context.WsdlPort.Service.ServiceDescription;
            List<Operation> list = new List<Operation>();
            List<Message> list2 = new List<Message>();
            foreach (PortType type in serviceDescription.PortTypes)
            {
                foreach (Operation operation in type.Operations)
                {
                    if (!IsAValidServiceContract(type.Name, operation.Name))
                    {
                        list.Add(operation);
                        foreach (Message message in serviceDescription.Messages)
                        {
                            if (StringComparer.Ordinal.Equals(message.Name, operation.Messages.Input.Message.Name))
                            {
                                list2.Add(message);
                            }
                            else if (StringComparer.Ordinal.Equals(message.Name, operation.Messages.Output.Message.Name))
                            {
                                list2.Add(message);
                            }
                        }
                        continue;
                    }
                }
                foreach (Operation operation2 in list)
                {
                    type.Operations.Remove(operation2);
                }
            }
            foreach (Message message2 in list2)
            {
                serviceDescription.Messages.Remove(message2);
            }
            RemoveRedundantOperations(context);
        }

        /// <summary>
        ///     Check if a service contract is valid
        /// </summary>
        /// <param name="serviceContract"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        private static bool IsAValidServiceContract(string serviceContract, string operation)
        {
            bool isValid = false;
            if (string.IsNullOrEmpty(serviceContract))
            {
                throw new ArgumentNullException("serviceContract");
            }
            if (string.IsNullOrEmpty(operation))
            {
                throw new ArgumentNullException("operation");
            }
            if (StringComparer.Ordinal.Equals(serviceContract, "IWSTrustFeb2005Async"))
            {
                return StringComparer.Ordinal.Equals(operation, "TrustFeb2005IssueAsync");
            }
            if (StringComparer.Ordinal.Equals(serviceContract, "IWSTrust13Async"))
            {
                isValid = StringComparer.Ordinal.Equals(operation, "Trust13IssueAsync");
            }
            if (StringComparer.Ordinal.Equals(serviceContract, "IWSTrust13Sync"))
            {
                isValid = StringComparer.Ordinal.Equals(operation, "Trust13Issue");
            }
            return isValid;
        }

        /// <summary>
        ///     This method is to remove redundant operations
        /// </summary>
        /// <param name="context"></param>
        private void RemoveRedundantOperations(WsdlEndpointConversionContext context)
        {
            List<OperationBinding> list = new List<OperationBinding>();
            foreach (OperationBinding binding in context.WsdlBinding.Operations)
            {
                if (_unSupportedOperations.ContainsKey(binding.Name))
                {
                    list.Add(binding);
                }
            }
            foreach (OperationBinding binding2 in list)
            {
                context.WsdlBinding.Operations.Remove(binding2);
                RemoveRedundantOperationPolicy(context, binding2);
            }
        }

        /// <summary>
        ///     This method is to remove reduandant operation policy
        /// </summary>
        /// <param name="context"></param>
        /// <param name="operationBinding"></param>
        private static void RemoveRedundantOperationPolicy(WsdlEndpointConversionContext context,
            OperationBinding operationBinding)
        {
            string attribute = null;
            string str2 = null;
            foreach (object obj2 in operationBinding.Input.Extensions)
            {
                XmlElement element = obj2 as XmlElement;
                if (((element != null) &&
                     StringComparer.Ordinal.Equals(element.NamespaceURI, "http://schemas.xmlsoap.org/ws/2004/09/policy")) &&
                    StringComparer.Ordinal.Equals(element.LocalName, "PolicyReference"))
                {
                    attribute = element.GetAttribute("URI");
                    break;
                }
            }
            foreach (object obj3 in operationBinding.Output.Extensions)
            {
                XmlElement element2 = obj3 as XmlElement;
                if (((element2 != null) &&
                     StringComparer.Ordinal.Equals(element2.NamespaceURI, "http://schemas.xmlsoap.org/ws/2004/09/policy")) &&
                    StringComparer.Ordinal.Equals(element2.LocalName, "PolicyReference"))
                {
                    str2 = element2.GetAttribute("URI");
                    break;
                }
            }
            object extension = null;
            object obj5 = null;
            foreach (object obj6 in context.WsdlBinding.ServiceDescription.Extensions)
            {
                XmlElement element3 = obj6 as XmlElement; //the first extension object -> remove Transport
                var transportNode = element3.GetElementsByTagName("TransportBinding",
                    "http://schemas.xmlsoap.org/ws/2005/07/securitypolicy");
                if (transportNode.Count > 0)
                {
                    element3.FirstChild.FirstChild.RemoveChild(transportNode[0]);
                    continue;
                }
                if (((element3 != null) &&
                     StringComparer.Ordinal.Equals(element3.NamespaceURI, "http://schemas.xmlsoap.org/ws/2004/09/policy")) &&
                    StringComparer.Ordinal.Equals(element3.LocalName, "Policy"))
                {
                    string str3 = element3.GetAttribute("Id",
                        "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
                    if (!string.IsNullOrEmpty(str3))
                    {
                        if (((extension == null) && (attribute != null)) &&
                            StringComparer.Ordinal.Equals(attribute.Substring(1), str3))
                        {
                            extension = obj6;
                        }
                        else if (((obj5 == null) && (str2 != null)) &&
                                 StringComparer.Ordinal.Equals(str2.Substring(1), str3))
                        {
                            obj5 = obj6;
                        }
                    }
                }
            }
            if (extension != null)
            {
                context.WsdlBinding.ServiceDescription.Extensions.Remove(extension);
            }
            if (obj5 != null)
            {
                context.WsdlBinding.ServiceDescription.Extensions.Remove(obj5);
            }
        }
    }
}