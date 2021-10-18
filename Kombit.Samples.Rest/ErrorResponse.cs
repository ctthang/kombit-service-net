using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kombit.Samples.Rest
{
    public class ErrorResponse
    {
        public ErrorResponse(int status, string error)
        {
            Status = status;
            Error = error;
        }

        public int Status { get; private set; }
        public string Error { get; private set; }
    }
}