using Fitz.Core.Services;
using Fitz.Features.Accounts;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Rename
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RenameService>();
        }
    }
}