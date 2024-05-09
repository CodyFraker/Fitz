using Fitz.Core.Services;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Fitz.Features.Music
{
    internal class Registrant : IServiceRegistrant
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLavalink();
            services.ConfigureLavalink(config =>
            {
                config.BaseAddress = new Uri(Environment.GetEnvironmentVariable("LAVALINK_ADDR"));
                config.WebSocketUri = new Uri($"ws://{Environment.GetEnvironmentVariable("LAVALINK_WEBSOCKET")}");
                config.ReadyTimeout = TimeSpan.FromSeconds(10);
                config.Passphrase = Environment.GetEnvironmentVariable("LAVALINK_PASS");
                config.HttpClientName = "LavaLinkHttpClient";
            });
        }
    }
}