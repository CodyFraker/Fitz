using Fitz.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Features.FAQ
{
    internal class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Manager>();
        }
    }
}
