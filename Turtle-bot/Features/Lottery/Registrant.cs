using Fitz.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Lottery
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<LotteryService>();
            services.AddSingleton<LotteryJob>();
        }
    }
}