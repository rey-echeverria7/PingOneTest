

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Palig.ICSS.support.Models;


namespace Palig.ICSS.support.JWT.Interfaces
{
    public interface ITokenGenerator
    {
        string GenerateTokenJwt(Dictionary<string, string> claims);
        public bool ValidateCurrentToken(string token);
        public string GetClaim(string token, string claimType);
        string GetToKemRequest(HttpRequest request, string header);

        string GetToKemResponse(HttpResponse request, string header);
        void RefrescarTokemAuth(ActionExecutedContext context);
        string GenerarTokenAuth(PingOneAuth request);

        void RefrescarTokem(ActionExecutedContext context);

        bool TokenExpired(string token);
    }
}
