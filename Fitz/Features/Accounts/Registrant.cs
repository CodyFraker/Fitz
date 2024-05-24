using Fitz.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Accounts
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<AccountService>();
        }
    }
}