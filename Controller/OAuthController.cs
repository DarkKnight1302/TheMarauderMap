using Microsoft.AspNetCore.Mvc;

namespace TheMarauderMap.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        [HttpGet]
        [Route("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            // Process the authorization code
            // Exchange the code for an access token
            return Ok($"OAuth2 callback successful {code}");
        }

    }
}
