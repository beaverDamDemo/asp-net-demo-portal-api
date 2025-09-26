using Microsoft.AspNetCore.Mvc;

namespace AspNetDemoPortalAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        public string Get() => "API is running.";

        [HttpGet("health")]
        public string healthCheck()
        {
            return "✅ Server is healthy";
        }
    }
}
