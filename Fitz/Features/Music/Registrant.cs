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
            var lavalinkAddr = Environment.GetEnvironmentVariable("LAVALINK_ADDR");
            var lavalinkWebSocket = Environment.GetEnvironmentVariable("LAVALINK_WEBSOCKET");
            
            if (string.IsNullOrWhiteSpace(lavalinkAddr) || string.IsNullOrWhiteSpace(lavalinkWebSocket))
            {
                return;
            }
            
            services.AddLavalink();
            services.ConfigureLavalink(config =>
            {
                config.BaseAddress = new Uri(lavalinkAddr);
                config.WebSocketUri = new Uri($"ws://{lavalinkWebSocket}");
                config.ReadyTimeout = TimeSpan.FromSeconds(10);
                config.Passphrase = Environment.GetEnvironmentVariable("LAVALINK_PASS");
                config.HttpClientName = "LavaLinkHttpClient";
            });
        }
    }
}