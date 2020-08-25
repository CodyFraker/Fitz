namespace Fitz.Guilds.Events
{
    using System.Threading.Tasks;
    using Fitz.BackgroundServices.Models;
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using Serilog;

    public class ChannelEvents : IEventHandler
    {
        private readonly DiscordClient dClient;

        public ChannelEvents(DiscordClient dClient)
        {
            this.dClient = dClient;
        }

        public void RegisterListeners()
        {
            //this.dClient.ChannelPinsUpdated += this.OnChannelPinsUpdatedAsync;
            this.dClient.GuildAvailable += DClient_GuildAvailable;
        }

        private Task DClient_GuildAvailable(GuildCreateEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
