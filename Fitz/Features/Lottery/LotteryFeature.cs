using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Core.Services.Settings;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Features.Lottery.Commands;
using Fitz.Variables;
using Hangfire;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public class LotteryFeature(
        SettingsService settingsService,
        DiscordClient dClient,
        AccountService accountService,
        LotteryService lotteryService,
        BankService bankService,
        BotLog botLog) : Feature
    {
        private readonly LotteryJob lottoJob = new LotteryJob(dClient, lotteryService, bankService, accountService, botLog, settingsService);
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private readonly DiscordClient dClient = dClient;

        public override string Name => "Lottery";

        public override string Description => "The great beer loterry";

        public override Task Disable()
        {
            // WE CANNOT UNREGISTER SLASH COMMANDS.
            this.cNext.UnregisterCommands<LotteryAdminCommands>();
            RecurringJob.RemoveIfExists("LotteryJob");
            return base.Disable();
        }

        public override Task Enable()
        {
            this.dClient.MessageCreated += this.onLotteryChannelMessageSent;
            this.slash.RegisterCommands<LotterySlashCommands>(Guilds.Waterbear);
            this.cNext.RegisterCommands<LotteryAdminCommands>();
            RecurringJob.AddOrUpdate("LotteryJob", () => this.lottoJob.Execute(), this.lottoJob.Interval);
            return base.Enable();
        }

        private async Task onLotteryChannelMessageSent(DiscordClient sender, MessageCreateEventArgs args)
        {
            // Check to see if we're in the lottery channel
            if (args.Channel.Id != Variables.Channels.Waterbear.LotteryInfo)
            {
                return;
            }
            else
            {
                // Check to see if the message is a command
                if (args.Message.MessageType == DSharpPlus.Entities.DiscordMessageType.ApplicationCommand || args.Author.IsBot)
                {
                    return;
                }
                else
                {
                    await args.Message.DeleteAsync();
                }
            }
        }
    }
}