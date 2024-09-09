using Microsoft.AspNetCore.Mvc;
using TheMarauderMap.Services.Interfaces;

namespace TheMarauderMap.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly ILogger<OAuthController> logger;
        private readonly IUserLoginService _userLoginService;
        public OAuthController(ILogger<OAuthController> logger, IUserLoginService userLoginService)
        {
            this.logger = logger;
            this._userLoginService = userLoginService;
        }

        [HttpGet]
        [Route("callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            // Process the authorization code
            // Exchange the code for an access token
            this.logger.LogInformation($"Recieved callback token code {code}");
            this._userLoginService.SetUserLoginCode(code, state);
            return Ok($"OAuth2 callback successful {code} : {state}");
        }

    }
}
