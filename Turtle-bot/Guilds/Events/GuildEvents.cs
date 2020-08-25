namespace Fitz.Guilds.Events
{
    using System.Threading.Tasks;
    using Fitz.BackgroundServices.Models;
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using Serilog;

    public class GuildEvents : IEventHandler
    {
        private readonly DiscordClient dClient;

        public GuildEvents(DiscordClient dClient)
        {
            this.dClient = dClient;
        }

        public void RegisterListeners()
        {
            this.dClient.GuildAvailable += this.OnGuildAvailableAsync;
        }

        private async Task OnGuildAvailableAsync(GuildCreateEventArgs args)
        {
            await args.Guild.RequestMembersAsync().ConfigureAwait(false);
        }
    }
}
