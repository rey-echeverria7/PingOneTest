

namespace Palig.ICSS.support.Models
{
    public class JWTOptions
    {
        public const string JWT = "JWT";
        public string JWT_ISSUER_AUDIENCE { get; set; } = string.Empty;
        public string JWT_ISSUER_TOKEN { get; set; } = string.Empty;
        public int JWT_DURATION_TOKEN { get; set; }
        public string JWT_SECRET_KEY { get; set; } = string.Empty;
        public int JWT_EXPIRE_MINUTES { get; set; }
    }
}
