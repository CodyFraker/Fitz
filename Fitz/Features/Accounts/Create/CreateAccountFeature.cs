using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Create.Discord;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Accounts.Jobs;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Variables;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Create;

public class CreateAccountFeature : Feature
{
    private readonly AccountService _accountService;
    private readonly DiscordClient _discordClient;
    private readonly JobManager _jobManager;
    private readonly SlashCommandsExtension _slash;
    private readonly CommandsNextExtension _cNext;
    private readonly BotLog _botLog;
    private readonly AccountJob _accountJob;

    public CreateAccountFeature(AccountService accountService, DiscordClient discordClient, JobManager jobManager, BotLog botLog, AccountJob accountJob)
    {
        _accountService = accountService;
        _discordClient = discordClient;
        _jobManager = jobManager;
        _slash = discordClient.GetSlashCommands();
        _cNext = discordClient.GetCommandsNext();
        _botLog = botLog;
        _accountJob = accountJob;
    }

    public override string Name => "Accounts";

    public override string Description => "Enables users to create accounts with the bot.";

    public override async Task Enable()
    {
        _jobManager.AddJob(_accountJob);

        _slash.RegisterCommands<AccountSlashCommands>(Guilds.Waterbear);

        // Check to see if Fitz has an account registered in the database.
        var fitzAccount = _accountService.FindAccount(Users.Fitz);
        if (fitzAccount == null)
        {
            var response = await _accountService.CreateAccountAsync(await _discordClient.GetUserAsync(Users.Fitz));
            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                _botLog.Error($"Failed to create Fitz account: {response.Message}");
            }
        }

        // Register event handlers
        _discordClient.GuildMemberAdded += HandleGuildMemberAdded;
        _discordClient.GuildMemberRemoved += HandleGuildMemberRemoved;
        _discordClient.GuildMemberUpdated += HandleGuildMemberUpdated;

        await base.Enable();
    }

    public override async Task Disable()
    {
        _jobManager.RemoveJob(_accountJob);
        
        // Unregister commands
        _slash.RegisterCommands<AccountSlashCommands>();

        // Unregister event handlers
        _discordClient.GuildMemberAdded -= HandleGuildMemberAdded;
        _discordClient.GuildMemberRemoved -= HandleGuildMemberRemoved;
        _discordClient.GuildMemberUpdated -= HandleGuildMemberUpdated;

        await base.Disable();
    }

    private async Task HandleGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
    {
        if (e.Guild.Id != Guilds.Waterbear)
            return;

        var response = await _accountService.CreateAccountAsync(e.Member);
        if (response.StatusCode != System.Net.HttpStatusCode.Created)
        {
            _botLog.Error($"Failed to create account for {e.Member.Username}: {response.Message}");
        }
    }

    private async Task HandleGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
    {
        if (e.Guild.Id != Guilds.Waterbear)
            return;

        var account = _accountService.FindAccount(e.Member.Id);
        if (account != null)
        {
            account.Deactivated = true;
            var command = new UpdateAccountCommand
            {
                Id = account.Id,
                Deactivated = true
            };
            await _accountService.SetFavorabilityAsync(account, account.Favorability);
        }
    }

    private async Task HandleGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
    {
        if (e.Guild.Id != Guilds.Waterbear)
            return;

        var account = _accountService.FindAccount(e.Member.Id);
        if (account != null)
        {
            account.Username = e.Member.Username;
            var command = new UpdateAccountCommand
            {
                Id = account.Id,
                Username = e.Member.Username
            };
            await _accountService.SetFavorabilityAsync(account, account.Favorability);
        }
    }
}