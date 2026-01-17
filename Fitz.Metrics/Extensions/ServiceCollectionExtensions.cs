using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Metrics.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFitzMetrics(this IServiceCollection services)
        {
            services.AddSingleton<FitzMetrics>();
            return services;
        }
    }
}
