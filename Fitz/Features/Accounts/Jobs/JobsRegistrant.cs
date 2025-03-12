using Fitz.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Accounts.Jobs
{
    public class JobsRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<AccountJob>();
        }
    }
} 