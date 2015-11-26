using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace Kombit.Samples.STS.Code
{
    public class ExceptionThownHandler : IErrorHandler
    {
        public bool HandleError(Exception error)
        {
            if (error == null)
                return false;
            return true;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message msg)
        {
            if (error == null)
                return;

            var code = "5050";
            var faultException = error as FaultException;
            if (faultException != null)
            {
                //return;
                code = faultException.Code.Name;
            }

            //var customFault = new FaultException(new FaultReason(error.Message),
            //        new FaultCode("5050"));
            var customFault = new FaultException(new FaultReason(error.Message),
                    new FaultCode(code));

            // Create the message fault
            MessageFault mf = customFault.CreateMessageFault();

            // Update reference to point to the message
            msg = Message.CreateMessage(version, mf,
                "http://docs.oasis-open.org/ws-sx/ws-trust/200512/RST/Fault");
        }
    }
}