using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Palig.ICSS.support.Models;

namespace Palig.ICSS.support.Extensions
{
    public static class OptionsServiceExtension
    {
        public static IServiceCollection AddOptionServiceCustom(this IServiceCollection services, IConfiguration configuration)
        {

            services.Configure<JWTOptions>(configuration.GetSection(JWTOptions.JWT));
            services.Configure<SecurityOptions>(configuration.GetSection(SecurityOptions.Security));


            return services;
        }
    }
}
