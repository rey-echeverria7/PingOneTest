using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Palig.ICSS.support.Constants;
using Palig.ICSS.support.Helper.Extensions;
using Palig.ICSS.support.JWT.Interfaces;
using Palig.ICSS.support.Models;
using Palig.ICSS.support.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Palig.ICSS.support.JWT.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly JWTOptions _jWTOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAESHelper _aES;
        private readonly SecurityOptions _securityOptions;




        public TokenGenerator(IOptionsSnapshot<SecurityOptions> servicesOptions,
            IOptionsSnapshot<JWTOptions> jWTOptions,
            IHttpContextAccessor httpContextAccessor,
            IAESHelper aES,
            IOptions<SecurityOptions> securityOptions)
        {
            _jWTOptions = jWTOptions.Value;
            _httpContextAccessor = httpContextAccessor;
            _aES = aES;
            _securityOptions = securityOptions.Value;
        }


        public bool TokenExpired(string token)
        {
            var stream = token;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            JwtSecurityToken tokenS = handler.ReadToken(stream) as JwtSecurityToken;

            DateTime dateTimeToken = DateTime.UtcNow;

            if (dateTimeToken > tokenS.ValidTo)
                return true;

            return false;
        }


        public bool ValidateCurrentToken(string token)
        {
            var secretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(_jWTOptions.JWT_SECRET_KEY));
            var audienceToken = _jWTOptions.JWT_ISSUER_AUDIENCE;
            var issuerToken = _jWTOptions.JWT_ISSUER_TOKEN;
            var expireTime = _jWTOptions.JWT_EXPIRE_MINUTES;

            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var result = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = issuerToken,
                    ValidAudience = audienceToken,
                    IssuerSigningKey = securityKey
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string GetClaim(string token, string claimType)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                var stringClaimValue = securityToken.Claims.First(claim => claim.Type == claimType).Value;
                return Encoding.UTF8.GetString(Convert.FromBase64String(stringClaimValue));
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetToKemRequest(HttpRequest request, string header)
        {
            try
            {
                var TOKEN = string.Empty;

                string authHeaders = request.Headers[header];
                if (string.IsNullOrEmpty(authHeaders))
                    return string.Empty;

                string bearerToken = authHeaders;
                TOKEN = bearerToken.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase) ? bearerToken.Substring(7) : bearerToken;

                return TOKEN;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public string GetToKemResponse(HttpResponse request, string header)
        {
            try
            {
                var TOKEN = string.Empty;

                string authHeaders = request.Headers[header];
                if (string.IsNullOrEmpty(authHeaders))
                    return string.Empty;

                string bearerToken = authHeaders;
                TOKEN = bearerToken.StartsWith("Bearer ", StringComparison.CurrentCultureIgnoreCase) ? bearerToken.Substring(7) : bearerToken;

                return TOKEN;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public string GenerateTokenJwt(Dictionary<string, string> claims)
        {
            var secretKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(_jWTOptions.JWT_SECRET_KEY));
            var audienceToken = _jWTOptions.JWT_ISSUER_AUDIENCE;
            var issuerToken = _jWTOptions.JWT_ISSUER_TOKEN;
            var expireTime = _jWTOptions.JWT_EXPIRE_MINUTES;

            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);



            List<Claim> arr = new List<Claim>();

            if (claims.Count > 0)
            {
                arr = new List<Claim>();

                foreach (var item in claims)
                {
                    Claim claim = new Claim(item.Key, item.Value);
                    arr.Add(claim);
                }
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(arr);


            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtSecurityToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(expireTime)),
                signingCredentials: signingCredentials);

            var jwtTokenString = tokenHandler.WriteToken(jwtSecurityToken);


            return jwtTokenString;
        }

        public void RefrescarTokemAuth(ActionExecutedContext context)
        {

            try
            {
                var Obtenertokem = GetToKemRequest(context.HttpContext.Request, "Authorization");


                if (string.IsNullOrEmpty(Obtenertokem))
                    return;

                Obtenertokem = Obtenertokem.DecodeBase64();

                var PingOneAuth = GetClaim(Obtenertokem, ClaimsCustom.PingOneAuth);

                var DictionaryClaims = new Dictionary<string, string>();

                DictionaryClaims.Add(ClaimsCustom.PingOneAuth, PingOneAuth.EncodeBase64());



                var tokem = GenerateTokenJwt(DictionaryClaims);

                if (!context.HttpContext.Response.HasStarted)
                {
                    context.HttpContext.Response.Headers.Remove("Access-Control-Expose-Headers");
                    context.HttpContext.Response.Headers.Remove("Authorization");

                    context.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Authorization");
                    context.HttpContext.Response.Headers.Add("Authorization", tokem.EncodeBase64());
                }
            }
            catch
            {
                throw;
            }
        }


        public string GenerarTokenAuth(PingOneAuth request)
        {
            try
            {




                var DictionaryClaims = new Dictionary<string, string>();


                DictionaryClaims.Add(ClaimsCustom.PingOneAuth, _aES.Encrypt(JsonConvert.SerializeObject(request)).EncodeBase64());


                var token = GenerateTokenJwt(DictionaryClaims);

                _httpContextAccessor.HttpContext.Response.Headers.Remove("Access-Control-Expose-Headers");
                _httpContextAccessor.HttpContext.Response.Headers.Remove("Authorization");
                _httpContextAccessor.HttpContext.Request.Headers.Remove("Authorization");

                _httpContextAccessor.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Authorization");
                _httpContextAccessor.HttpContext.Response.Headers.Add("Authorization", token.EncodeBase64());
                _httpContextAccessor.HttpContext.Request.Headers.Add("Authorization", token.EncodeBase64());


                return token.EncodeBase64();
            }
            catch
            {
                throw;
            }
        }


        public void RefrescarTokem(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }
    }
}

