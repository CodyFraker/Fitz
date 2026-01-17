using Fitz.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Core.Api
{
    public class ApiClientRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient<FitzApiClient>();
            services.AddSingleton<FitzApiClient>();
        }
    }
}
