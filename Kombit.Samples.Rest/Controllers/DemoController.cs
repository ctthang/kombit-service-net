using System.Web.Http;

namespace Kombit.Samples.Rest.Controllers
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
