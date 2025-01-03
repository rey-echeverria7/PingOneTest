using Microsoft.AspNetCore.Mvc;

namespace PingOneTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpGet("login")]
        public IActionResult Login()
        {
            var redirectUri = "http://localhost:4200/auth/callback";
            var authUri = $"https://auth.pingone.com/89a7ea3f-4d4e-4273-a44a-dd9f5e82e293/as/authorize?client_id=7745681e-3c34-4cf0-84c6-3bafe4d95c7a&response_type=code&scope=openid profile&redirect_uri={redirectUri}";
            return Redirect(authUri);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] AuthCallbackRequest request)
        {
            // Exchange authorization code for access token
            var tokenUrl = $"https://auth.pingone.com/89a7ea3f-4d4e-4273-a44a-dd9f5e82e293/as/token";
            var clientId = "7745681e-3c34-4cf0-84c6-3bafe4d95c7a";
            var clientSecret = "iAi1m.l7rHCeaatj0OatsljvD53Bgt1CNBA1eFQDwtZTq-x7TicQDRbgULsG5w3N";

            using var client = new HttpClient();
            var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", request.Code },
            { "redirect_uri", "http://localhost:4200/auth/callback" },
            { "client_id", clientId },
            { "client_secret", clientSecret }
        }));

            var content = await response.Content.ReadAsStringAsync();
            return Ok(content);
        }
    }

    public class AuthCallbackRequest
    {
        public string Code { get; set; }
    }
}
