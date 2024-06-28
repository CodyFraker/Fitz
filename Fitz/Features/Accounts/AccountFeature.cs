using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Jobs;
using Fitz.Features.Accounts.Models;
using Fitz.Variables;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public class UserAccountFeature(DiscordClient dClient, AccountService accountService, JobManager jobManager, BotLog botLog) : Feature
    {
        private readonly JobManager jobManager = jobManager;
        private readonly AccountJob accountJob = new AccountJob(accountService, dClient, botLog);
        private readonly SlashCommandsExtension slash = dClient.GetSlashCommands();
        private readonly CommandsNextExtension cNext = dClient.GetCommandsNext();
        private AccountService accountService = accountService;
        private readonly BotLog botlog = botLog;
        private readonly DiscordClient dClient = dClient;

        public override string Name => "Accounts";

        public override string Description => "Enable users to create accounts associated with the bot.";

        public override Task Disable()
        {
            this.jobManager.RemoveJob(this.accountJob);
            this.cNext.UnregisterCommands<AccountAdminSlashCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.jobManager.AddJob(this.accountJob);

            // For some reason, discord isn't wanting to register the command globally.
            // Hence why I register the commands in two guilds here.
            this.slash.RegisterCommands<AccountSlashCommands>(Guilds.Waterbear);
            //this.slash.RegisterCommands<AccountSlashCommands>(Guilds.DodeDuke);
            this.slash.RegisterCommands<AccountAdminSlashCommands>();

            // Check to see if Fitz has an account registered in the database.
            if (accountService.FindAccount(Users.Fitz) == null)
            {
                accountService.CreateFitzAccountAsync();
            }

            this.dClient.GuildMemberRemoved += this.GuildMemberRemoved;
            this.dClient.GuildMemberAdded += this.GuildMemberAdded;

            return base.Enable();
        }

        private async Task GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            // Check to see if the user has an account. If so, mark as active.
            Account account = accountService.FindAccount(args.Member.Id);
            if (account != null)
            {
                await accountService.SetDeactivatedAsync(account, !account.Deactivated);
            }
            else
            {
                try
                {
                    DiscordDmChannel dmChannel = await args.Member.CreateDmChannelAsync();
                    await dmChannel.SendMessageAsync("Hey, I'm Fitz. If you want to get the most out of the server, run `/signup`.");
                    botlog.Information(LogConsoleSettings.AccountLog, $"Sent the welcome message to {args.Member.Username} via DM.");
                }
                catch (Exception e)
                {
                    botlog.Information(LogConsoleSettings.AccountLog, $"Failed to send the welcome message to {args.Member.Username} via DM. User might have blocked the bot. {e.Message}");
                }
            }
        }

        private async Task GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
        {
            // Check to see if the user has an account. If so, mark as inactive.
            Account account = accountService.FindAccount(args.Member.Id);
            if (account != null)
            {
                await accountService.SetDeactivatedAsync(account, !account.Deactivated);
            }
            else
            {
                return;
            }
        }
    }
}