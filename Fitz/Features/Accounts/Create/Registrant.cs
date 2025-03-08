using Fitz.Core.Services;
using Fitz.Features.Accounts.Create.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Accounts.Create;

public class Registrant : IServiceRegistrant
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<CreateAccountService>();
        services.AddSingleton<CreateAccountRepository>();
    }
}