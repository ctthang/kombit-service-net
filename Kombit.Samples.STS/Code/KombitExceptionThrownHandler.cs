using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Kombit.Samples.STS.Code
{
    public class KombitExceptionThrownHandler : BehaviorExtensionElement, IServiceBehavior
    {
        /// <summary>
        /// Get behavior type
        /// </summary>
        public override Type BehaviorType
        {
            get { return GetType(); }
        }

        /// <summary>
        /// Get behavior
        /// </summary>
        /// <returns></returns>
        protected override object CreateBehavior()
        {
            return this;
        }

        /// <summary>
        /// Get instance of error handler which is ExceptionhrownHandler in this sample
        /// </summary>
        /// <returns></returns>
        private IErrorHandler GetInstance()
        {
            return new ExceptionThownHandler();
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Apply the ExceptionThrownHandler for service's dispatcher
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <param name="serviceHostBase"></param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            IErrorHandler errorHandlerInstance = GetInstance();
            foreach (ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers)
            {
                dispatcher.ErrorHandlers.Add(errorHandlerInstance);
            }
        }

        /// <summary>
        /// Validate service behavior
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <param name="serviceHostBase"></param>
        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            //do no thing
        }
    }
}