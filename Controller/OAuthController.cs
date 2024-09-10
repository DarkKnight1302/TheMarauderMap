using Blazored.SessionStorage;
using Microsoft.AspNetCore.Mvc;
using TheMarauderMap.Constants;
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
            try
            {
                this.logger.LogInformation($"Recieved callback token code {code} : {state}");
                await this._userLoginService.LoginUser(code, state);
                return Ok($"Login Successful!");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{ex.Message} \n {ex.StackTrace}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
