using Fitz.Core.Services;
using Fitz.Features.Music.Play.Discord;
using Fitz.Features.Music.Play.Domain;
using Fitz.Features.Music.Stop.Discord;
using Fitz.Features.Music.Stop.Domain;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fitz.Features.Music
{
    public class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Register Lavalink services
            services.AddLavalink();
            services.ConfigureLavalink(config =>
            {
                config.BaseAddress = new Uri(Environment.GetEnvironmentVariable("LAVALINK_ADDR"));
                config.WebSocketUri = new Uri($"ws://{Environment.GetEnvironmentVariable("LAVALINK_WEBSOCKET")}");
                config.ReadyTimeout = TimeSpan.FromSeconds(10);
                config.Passphrase = Environment.GetEnvironmentVariable("LAVALINK_PASS");
                config.HttpClientName = "LavaLinkHttpClient";
            });

            // Register domain services
            services.AddScoped<PlayService>();
            services.AddScoped<StopService>();

            // Register Discord adapters
            services.AddScoped<PlayAdapter>();
            services.AddScoped<StopAdapter>();

            // Register the facade service
            services.AddScoped<MusicService>();

            // Register the feature
            services.AddSingleton<MusicFeature>();
        }
    }
}