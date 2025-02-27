using Fitz.Core.Services;
using Fitz.Features.Polls.Create.Discord;
using Fitz.Features.Polls.Create.Domain;
using Fitz.Features.Polls.Create.Persistance;
using Fitz.Features.Polls.Update.Discord;
using Fitz.Features.Polls.Update.Domain;
using Fitz.Features.Polls.Update.Persistance;
using Fitz.Features.Polls.Vote.Discord;
using Fitz.Features.Polls.Vote.Domain;
using Fitz.Features.Polls.Vote.Persistance;
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
            // Register domain services
            services.AddScoped<CreatePollService>();
            services.AddScoped<UpdatePollService>();
            services.AddScoped<VoteService>();

            // Register repositories
            services.AddScoped<CreatePollRepository>();
            services.AddScoped<UpdatePollRepository>();
            services.AddScoped<VoteRepository>();

            // Register Discord adapters
            services.AddScoped<CreatePollAdapter>();
            services.AddScoped<UpdatePollAdapter>();
            services.AddScoped<VoteAdapter>();

            // Register the facade service
            services.AddScoped<PollService>();

            // Register the feature
            services.AddSingleton<PollsFeature>();
        }
    }
}