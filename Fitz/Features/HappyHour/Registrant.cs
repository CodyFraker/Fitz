using Fitz.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.HappyHour
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HappyHourService>();
            services.AddSingleton<HappyHourJob>();
        }
    }
}