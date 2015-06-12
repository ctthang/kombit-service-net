using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Kombit.Samples.STS.Code
{
    [DataContract(Namespace = "http://kombit.sample.dk/fault")]

    public class StsFaultMessage
    {
        [DataMember]
        public string EventId { get; set; }

        [DataMember]

        public string Message { get; set; }

    }
}