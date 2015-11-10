using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Kombit.Samples.STS.Code
{
    [DataContract(Namespace = "https://sts.kombit.dk/fault")]

    public class StsFaultDetail
    {

        [DataMember]
        public string Message { get; set; }

    }
}