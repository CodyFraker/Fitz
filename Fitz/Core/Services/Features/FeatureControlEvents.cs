using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitz.Variables;
using Fitz.Variables.Channels;

namespace Fitz.Core.Services.Features
{
    public class FeatureControlEvents : Feature
    {
        private readonly DiscordClient dClient;
        private readonly FeatureManager featureManager;

        public FeatureControlEvents(DiscordClient dClient, FeatureManager featureManager)
        {
            this.dClient = dClient;
            this.featureManager = featureManager;
        }

        public override string Name => "FeatureControl";

        public override string Description => "Feature controls using embeds";

        public override bool Protected => true;

        public override Task Enable()
        {
            this.dClient.GuildAvailable += this.OnGuildAvailable;
            this.dClient.MessageReactionAdded += this.OnMessageReactionAdded;
            return base.Enable();
        }

        private static DiscordEmbed CreateFeatureEmbed(Feature feature)
        {
            return new DiscordEmbedBuilder
            {
                Title = feature.Name,
                Description = feature.Description,
                Timestamp = DateTime.UtcNow,
                Color = feature.Enabled ? new DiscordColor(21, 137, 255) : new DiscordColor(131, 126, 124),
            };
        }

        // Ignore timestamps
        private static bool IdenticalEmbed(DiscordEmbed a, DiscordEmbed b)
        {
            return a.Title == b.Title
                && a.Description == b.Description;
        }

        private async Task OnMessageReactionAdded(DiscordClient dClient, MessageReactionAddEventArgs args)
        {
            if (args.Guild.Id != Guilds.DodeDuke || args.Channel.Id != DodeDuke.Settings || args.User.Id == dClient.CurrentUser.Id)
            {
                return;
            }

            DiscordChannel settingsChannel = await this.dClient.GetChannelAsync(DodeDuke.Settings);
            DiscordChannel botMods = await this.dClient.GetChannelAsync(DodeDuke.BotMods);
            DiscordMessage featureMessage = await settingsChannel.GetMessageAsync(args.Message.Id);
            Feature feature = this.featureManager.Features.Where(f => f.Name == featureMessage.Embeds[0]?.Title).FirstOrDefault();

            if (feature == null)
            {
                return;
            }
            else if (args.Emoji.Id == FeatureEmojis.ToggleOff && feature.Enabled)
            {
                await feature.Disable();
                await this.featureManager.UpdateFeatureStatusAsync(feature.Name, false);
                await botMods.SendMessageAsync($"{args.User.Username}#{args.User.Discriminator} *disabled* `{feature.Name}` at {DateTime.Now}\n" +
                    $"{featureMessage.JumpLink}");
            }
            else if (args.Emoji.Id == FeatureEmojis.ToggleOn && !feature.Enabled)
            {
                await feature.Enable();
                await this.featureManager.UpdateFeatureStatusAsync(feature.Name, true);
                await botMods.SendMessageAsync($"{args.User.Username}#{args.User.Discriminator} *enabled* `{feature.Name}` at {DateTime.Now}\n" +
                    $"{featureMessage.JumpLink}");
            }

            await featureMessage.ModifyAsync(embed: CreateFeatureEmbed(feature));
            await featureMessage.DeleteReactionAsync(args.Emoji, args.User);
        }

        private async Task OnGuildAvailable(DiscordClient dClient, GuildCreateEventArgs args)
        {
            if (args.Guild.Id != Guilds.DodeDuke)
            {
                return;
            }

            await this.dClient.Guilds[Guilds.DodeDuke].GetEmojisAsync();

            DiscordChannel settingsChannel = await this.dClient.GetChannelAsync(DodeDuke.Settings);

            IAsyncEnumerable<DiscordMessage> messages = settingsChannel.GetMessagesAsync(this.featureManager.Features.Count);

            List<DiscordMessage> featureMessages = new List<DiscordMessage>();

            await foreach (DiscordMessage message in messages)
            {
                featureMessages.Add(message);
            }

            for (int i = 0; i < this.featureManager.Features.Count; i++)
            {
                Feature feature = this.featureManager.Features[i];
                DiscordMessage message = featureMessages.Where(m => m.Embeds[0]?.Title == feature.Name).FirstOrDefault();
                DiscordEmbed newEmbed = CreateFeatureEmbed(feature);

                if (message == null)
                {
                    message = await settingsChannel.SendMessageAsync(embed: newEmbed);
                    await message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(this.dClient, FeatureEmojis.ToggleOff));
                    await message.CreateReactionAsync(DiscordEmoji.FromGuildEmote(this.dClient, FeatureEmojis.ToggleOn));
                }
                else if (!IdenticalEmbed(message.Embeds[0], newEmbed))
                {
                    await message.ModifyAsync(embed: newEmbed);
                }
            }
        }
    }
}