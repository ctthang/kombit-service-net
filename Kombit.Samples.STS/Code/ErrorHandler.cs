using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Kombit.Samples.STS.Code
{
    public sealed class ErrorHandler : IErrorHandler
    {
        public void ProvideFault(Exception error,
                     MessageVersion version,
                     ref Message fault)
        {
            //If it's a FaultException already, then we have nothing to do
            if (error is FaultException)
                return;

            //Get the operation description; omitted for brevity
            OperationDescription operationDesc = new OperationDescription("Trust13Issue", new ContractDescription("StsFaultMessage", "http://kombit.sample.dk/fault"));
            object faultDetail = GetFaultDetail(operationDesc.SyncMethod,
                        operationDesc.Faults,
                        error);
            if (faultDetail != null)
            {
                Type faultExceptionType =
                    typeof(FaultException<>).MakeGenericType(faultDetail.GetType());
                FaultException faultException =
                    (FaultException)Activator.CreateInstance(
            faultExceptionType, faultDetail, error.Message);
                MessageFault faultMessage = faultException.CreateMessageFault();
                fault = Message.CreateMessage(version,
                          faultMessage,
                          faultException.Action);
            }
        }

        public bool HandleError(Exception error)
        {
            return true;
        }

        private object GetFaultDetail(MethodInfo method,
        FaultDescriptionCollection faults,
            Exception error)
        {
            if (method != null)
            {
                return new StsFaultMessage()
                {
                    EventId = "0",
                    Message = error.Message
                };
            }
            
            return null;
        }
        //Other members omitted for brevity
    }
}