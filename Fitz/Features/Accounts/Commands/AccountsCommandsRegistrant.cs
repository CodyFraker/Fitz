using Fitz.Core.Services;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Create.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Accounts.Commands
{
    public class AccountsCommandsRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register AccountService
            services.AddScoped<AccountService>();
            
            // Register CreateAccountService if not already registered
            services.AddScoped<CreateAccountService>();
        }
    }
} 