using Microsoft.AspNetCore.Mvc;

namespace TheMarauderMap.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly ILogger<OAuthController> logger;
        public OAuthController(ILogger<OAuthController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        [Route("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            // Process the authorization code
            // Exchange the code for an access token
            this.logger.LogInformation($"Recieved callback token code {code}");
            return Ok($"OAuth2 callback successful {code}");
        }

    }
}
