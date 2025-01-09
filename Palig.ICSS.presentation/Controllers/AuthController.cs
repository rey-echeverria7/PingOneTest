
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Palig.ICSS.support.JWT.Interfaces;
using Palig.ICSS.support.Models;


namespace Palig.ICSS.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenGenerator _tokenGenerator;

        public AuthController(IConfiguration configuration, ITokenGenerator tokenGenerator)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
        }

        [HttpGet("login")]
        public  IActionResult Login()
        {
            var clientId = _configuration["OIDC:ClientId"];
            var redirectUri = _configuration["OIDC:RedirectUri"]; 
            var authorizationEndpoint = _configuration["OIDC:AuthorizationEndpoint"];

            var state = Guid.NewGuid().ToString(); 
            var scope = "openid profile email";

            var authorizationUrl = $"{authorizationEndpoint}?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope={scope}&state={state}";

            return Redirect(authorizationUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string id_token = null, [FromQuery] string state = null)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new ApiResponse<object>(400, "Authorization code is missing."));
            }

            var tokenEndpoint = _configuration["OIDC:TokenEndpoint"];
            var clientId = _configuration["OIDC:ClientId"];
            var clientSecret = _configuration["OIDC:ClientSecret"];
            var redirectUri = _configuration["OIDC:RedirectUri"];

            using var httpClient = new HttpClient();

            var tokenRequest = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret)
        });

            var response = await httpClient.PostAsync(tokenEndpoint, tokenRequest);

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new ApiResponse<object>(400, "Custom unauthorized message"));
            }



            var tokenResponse = await response.Content.ReadAsStringAsync();

            var token = this._tokenGenerator.GenerarTokenAuth(JsonConvert.DeserializeObject<PingOneAuth>(tokenResponse));

            return Redirect($"http://localhost:4200/auth/callback?code={token}");
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RevokeTokenRequest revokeRequest)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("http://localhost:4200"); 
        }

       
    }

    public class RevokeTokenRequest
    {
        public string Token { get; set; }
    }
}
