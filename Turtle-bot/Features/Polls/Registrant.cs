using Fitz.Core.Services;
using Fitz.Features.Accounts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PollService>();
        }
    }
}