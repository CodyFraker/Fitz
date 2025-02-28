using Fitz.Core.Services;
using Fitz.Features.Bank.AddBalance.Discord;
using Fitz.Features.Bank.AddBalance.Domain;
using Fitz.Features.Bank.AddBalance.Persistance;
using Fitz.Features.Bank.GetBalance.Discord;
using Fitz.Features.Bank.GetBalance.Domain;
using Fitz.Features.Bank.GetBalance.Persistance;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Bank
{
    public class BankRegistrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IGetBalanceRepository, GetBalanceRepository>();
            services.AddScoped<IAddBalanceRepository, AddBalanceRepository>();
            services.AddScoped<Fitz.Features.Accounts.Domain.IAccountRepository, AddBalanceRepository>();
            services.AddScoped<Fitz.Features.Bank.AddBalance.Domain.IAccountRepository, AddBalanceRepository>();
            services.AddScoped<Fitz.Features.Bank.AddBalance.Domain.ITransactionRepository, AddBalanceRepository>();
            
            // Register services
            services.AddScoped<GetBalanceService>();
            services.AddScoped<AddBalanceService>();
            services.AddSingleton<BankService>();
            
            // Register adapters
            services.AddScoped<GetBalanceAdapter>();
            services.AddScoped<AddBalanceAdapter>();
            services.AddScoped<AdminBalanceAdapter>();
            
            // Register feature
            services.AddSingleton<BankFeature>();
        }
    }
} 