using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Kombit.Samples.STS.Code
{
    public sealed class ErrorHandlingBehaviorAttribute : Attribute, IServiceBehavior
    {
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
    {
        foreach (ChannelDispatcherBase chanDispBase in
         serviceHostBase.ChannelDispatchers)
        {
            ChannelDispatcher channelDispatcher =
        chanDispBase as ChannelDispatcher;
            if (channelDispatcher == null)
                continue;
            channelDispatcher.ErrorHandlers.Add(new ErrorHandler(...));
        }
    }
        //Other interface methods omitted for brevity
    }
}