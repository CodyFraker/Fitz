namespace Fitz.Guilds.Events
{
    using System.Threading.Tasks;
    using Fitz.BackgroundServices;
    using Fitz.BackgroundServices.Models;
    using Fitz.Models;
    using DSharpPlus;
    using DSharpPlus.EventArgs;

    public class UserEvents : IEventHandler
    {
        private readonly DiscordClient dClient;
        private readonly FitzContextFactory dbFactory;
        private readonly ActivityManager activityManager;

        public UserEvents(DiscordClient dClient, FitzContextFactory dbFactory, FitzLog log, ActivityManager activityManager)
        {
            this.dClient = dClient;
            this.dbFactory = dbFactory;
            this.activityManager = activityManager;
        }

        public void RegisterListeners()
        {
            this.dClient.GuildMemberAdded += this.OnGuildMemberAddedAsync;
            //this.dClient.GuildMemberRemoved += this.OnGuildMemberRemovedAsync;
            //this.dClient.PresenceUpdated += this.ManageNowPlayingAsync;
            //this.dClient.PresenceUpdated += this.ManageNowStreamingUsers;
        }

        private async Task OnGuildMemberAddedAsync(DiscordClient client, GuildMemberAddEventArgs args)
        {
            // Do stuff here
        }
    }
}