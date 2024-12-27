using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace PingOneTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            //var redirectUrl = Url.Action(nameof(LoginCallback), "Account");
            //var redirectUrl = $"{HttpContext.Request.Scheme}://localhost:5115/api/account/login-callback";
            var redirectUrl = "http://localhost:4200/callback";
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