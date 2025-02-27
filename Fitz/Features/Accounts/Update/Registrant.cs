using Fitz.Core.Services;
using Fitz.Features.Accounts.Update.Discord;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Features.Accounts.Update.Jobs;
using Fitz.Features.Accounts.Update.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Accounts.Update
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<UpdateAccountRepository>();
            
            // Register services
            services.AddScoped<UpdateAccountService>();
            
            // Register adapters
            services.AddScoped<UpdateAccountAdapter>();
            services.AddScoped<AdminAccountAdapter>();
            
            // Register jobs
            services.AddScoped<AccountActivityJob>();
        }
    }
} 