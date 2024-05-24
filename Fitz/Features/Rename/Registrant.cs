using Fitz.Core.Services;
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