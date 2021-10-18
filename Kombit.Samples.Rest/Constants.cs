using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Kombit.Samples.Rest
{
    public static class Constants
    {
        public static string JwtCvr
        {
            get { return ConfigurationManager.AppSettings["JwtCvr"]; }
        }

        public static string JwtIssuer
        {
            get { return ConfigurationManager.AppSettings["JwtIssuer"]; }
        }

        public static string JwtSigningCertificate
        {
            get { return ConfigurationManager.AppSettings["JwtSigningCertificate"]; }
        }
    }
}