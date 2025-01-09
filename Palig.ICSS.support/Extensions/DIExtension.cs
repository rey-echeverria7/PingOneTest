using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Palig.ICSS.support.Extensions
{
    public static class DIExtension
    {

        public static void AddDIExtension(this IServiceCollection services)
        {

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(s =>
                                                                                s.FullName != null && (
                                                                                    s.FullName.Contains("Palig.ICSS.api") ||
                                                                                    s.FullName.Contains("Palig.ICSS.application") ||
                                                                                    s.FullName.Contains("Palig.ICSS.data") ||
                                                                                    s.FullName.Contains("Palig.ICSS.presentation") ||
                                                                                    s.FullName.Contains("Palig.ICSS.support")
                                                                                )
                                                                            );
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsClass && !type.IsAbstract)
                    {
                        var interfaces = type.GetInterfaces();
                        foreach (var @interface in interfaces)
                        {
                            services.AddTransient(@interface, type);
                        }
                    }
                }
            }

           
        }
    }
}
