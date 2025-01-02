using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PingOneTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var redirectUrl = "http://localhost:4200/callback";
            _logger.LogInformation("Redirect URL: {RedirectUrl}", redirectUrl);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "OpenIdConnect");
        }

        [HttpGet("login-callback")]
        public async Task<IActionResult> LoginCallback()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();
            if (!authenticateResult.Succeeded)
                return BadRequest();

            // Redirect after successful login
            return Redirect("http://localhost:4200");
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("http://localhost:4200");
        }
    }
}