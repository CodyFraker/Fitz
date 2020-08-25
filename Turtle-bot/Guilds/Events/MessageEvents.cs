namespace Fitz.Guilds.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Fitz.BackgroundServices.Models;
    using Fitz.Models;
    using Fitz.Variables;
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;

    public class MessageEvents : IEventHandler
    {

        private readonly DiscordClient dClient;
        private readonly FitzContextFactory dbFactory;

        public MessageEvents(DiscordClient dClient, FitzContextFactory dbFactory)
        {
            this.dClient = dClient;
            this.dbFactory = dbFactory;
        }

        public void RegisterListeners()
        {
            this.dClient.MessageCreated += this.OnMessageCreatedAsync;
        }

        private async Task OnMessageCreatedAsync(MessageCreateEventArgs args)
        {
            // Ignore the message if the author is a Fitz account.
            if (args.Author.IsBot == true)
            {
                return;
            }
        }
    }
}
