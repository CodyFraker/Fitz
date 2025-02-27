using Fitz.Core.Services;
using Fitz.Features.Rename.Notify.Domain;
using Fitz.Features.Rename.Notify.Persistance;
using Fitz.Features.Rename.Notify.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace Fitz.Features.Rename
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RenameService>();
            
            // Register Notify components
            services.AddSingleton<NotifyRenameService>();
            services.AddSingleton<NotifyRenameRepository>();
            services.AddSingleton<NotifyRenameAdapter>();
        }
    }
}