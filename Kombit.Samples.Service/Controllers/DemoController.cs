using Kombit.Samples.Service.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Kombit.Samples.Service.Controllers
{
    public class DemoController : ApiController
    {
        [HttpGet]
        [DemoRestApiAuthorizationFilter()]
        public IHttpActionResult Ping()
        {
            return Ok("Congratulations!");
        }
    }
}
