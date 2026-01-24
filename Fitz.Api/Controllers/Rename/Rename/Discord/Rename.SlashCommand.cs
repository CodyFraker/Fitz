using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Account.GetAccount.Domain;
using Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;
using Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;
using Fitz.Api.Controllers.Rename.CalculateRenameCost.Http;
using Fitz.Api.Controllers.Rename.CreateRename.Domain;
using Fitz.Api.Controllers.Rename.CreateRename.Http;
using Fitz.Api.Controllers.Rename.Exceptions;
using Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain;
using Fitz.Core.Commands.Attributes;
using Fitz.Database.Entities;
using ToMarkdownTable;
using System.Security.Cryptography;

namespace Fitz.Api.Controllers.Rename.Rename.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public sealed class RenameSlashCommand(
    CalculateRenameCostFacade calculateRenameCostFacade,
    CreateRenameFacade createRenameFacade,
    BuyoutRenamesFacade buyoutRenamesFacade,
    GetRenamesByUserFacade getRenamesByUserFacade,
    GetAccountFacade getAccountFacade,
    DiscordClient discordClient,
    ILogger<RenameSlashCommand> logger) : ApplicationCommandModule
{
    private readonly CalculateRenameCostFacade _calculateRenameCostFacade = calculateRenameCostFacade;
    private readonly CreateRenameFacade _createRenameFacade = createRenameFacade;
    private readonly BuyoutRenamesFacade _buyoutRenamesFacade = buyoutRenamesFacade;
    private readonly GetRenamesByUserFacade _getRenamesByUserFacade = getRenamesByUserFacade;
    private readonly GetAccountFacade _getAccountFacade = getAccountFacade;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<RenameSlashCommand> _logger = logger;

    [SlashCommand("rename", "Rename a user within the guild.")]
    [RequireAccount]
    public async Task Rename(InteractionContext ctx,
        [Option("User", "Manage whose account?")] DiscordUser user = null,
        [Option("Name", "What should their new name be?")] string newName = null,
        [Option("Days", "What should their new name be?")] double days = 1)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Rename command started via Discord slash command. UserId: {UserId}, Username: {Username}, TargetUser: {TargetUser}, NewName: {NewName}, Days: {Days}", 
            userId, username, user?.Id, newName, days);

        if (user == null)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You need to specify a user.")
                .AsEphemeral(true));
            return;
        }

        if (newName != null && newName.Length > 32)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("That name is too long. The max length of a name is 32 characters.")
                .AsEphemeral(true));
            return;
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You need to specify a new name for that user.")
                .AsEphemeral(true));
            return;
        }

        if (days <= 0 || days > 365)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("You need to specify a valid number of days. 1-365")
                .AsEphemeral(true));
            return;
        }

        try
        {
            var affectedUserCommand = GetAccountCommand.From(user.Id);
            var affectedUserResponse = await _getAccountFacade.Execute(affectedUserCommand, CancellationToken.None);

            var requestingUserCommand = GetAccountCommand.From(userId);
            var requestingUserResponse = await _getAccountFacade.Execute(requestingUserCommand, CancellationToken.None);

            var costCommand = CalculateRenameCostCommand.From(new CalculateRenameCostRequestDto
            {
                AffectedUserId = affectedUserResponse.Id,
                RequestedUserId = requestingUserResponse.Id,
                Days = days,
                NewName = newName
            });

            var costResponse = await _calculateRenameCostFacade.Execute(costCommand, CancellationToken.None);
            int renameCost = costResponse.Cost;

            if (requestingUserResponse.Beer < renameCost)
            {
                await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                    .WithContent($"Changing their name would require you have {renameCost}. You instead only have {requestingUserResponse.Beer}. idiot.")
                    .AsEphemeral(true));
                return;
            }

            int uniqueId = GenerateUniqueId();

            await ctx.DeferAsync(true);

            var renamesCommand = GetRenamesByUserCommand.From(affectedUserResponse.Id);
            var renamesResponse = await _getRenamesByUserFacade.Execute(renamesCommand, CancellationToken.None);
            var renames = renamesResponse.Renames.OrderByDescending(x => x.Expiration).ToList();

            DiscordButtonComponent acceptBtn = new(DiscordButtonStyle.Success, $"rename_confirm_{uniqueId}", "Confirm", false);
            DiscordButtonComponent cancelBtn = new(DiscordButtonStyle.Danger, $"rename_cancel_{uniqueId}", "Cancel", false);

            if (renames.Count == 0)
            {
                var startDate = DateTime.UtcNow;
                var expiration = startDate.AddDays(days);

                await ctx.FollowUpAsync(
                    new DiscordFollowupMessageBuilder()
                    .WithContent($"Renaming {user.Username} to {newName} for {days} day(s) will cost {renameCost} beer.\n" +
                    $"Start Date: {startDate}\n" +
                    $"Expiration Date: {expiration}\n" +
                    $"Do you want to proceed?")
                    .AddComponents(acceptBtn, cancelBtn)
                    .AsEphemeral(true));

                _discordClient.ComponentInteractionCreated += async (s, e) =>
                {
                    if (e.User.Id != userId) return;

                    if (e.Id == $"rename_confirm_{uniqueId}")
                    {
                        await HandleConfirmRename(ctx, e, user, newName, (int)days, renameCost, affectedUserResponse.Id, requestingUserResponse.Id, startDate, expiration, RenameStatusEnum.Active);
                    }
                    else if (e.Id == $"rename_cancel_{uniqueId}")
                    {
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder()
                            .WithContent("Rename cancelled."));
                    }
                };
            }
            else
            {
                int buyoutCost = renames.Sum(r => r.Cost) + renameCost;
                bool disableBuyOutBtn = requestingUserResponse.Beer < buyoutCost;

                DiscordButtonComponent buyOutBtn = new(DiscordButtonStyle.Primary, $"buyout_confirm_{uniqueId}", "Buy out", disableBuyOutBtn);
                DiscordButtonComponent acceptPendingBtn = new(DiscordButtonStyle.Success, $"rename_pending_confirm_{uniqueId}", "Confirm", false);

                string table = renames.Select(rename => new
                {
                    Name = rename.NewName,
                    Expires = rename.Expiration
                }).ToMarkdownTable();

                string content = disableBuyOutBtn
                    ? $"{user.Username} already has an active rename.\n" +
                      $"Press 'Confirm' if you'd wish your rename request to start after {renames[0].Expiration}EST\n" +
                      $"You do not have enough beer to buyout the pending and active renames.\n" +
                      $"Buying out the renames will cost you **{buyoutCost}** beer.\n" +
                      $"Press 'Cancel' to cancel this request." +
                      $"\n\n```{table}```"
                    : $"{user.Username} already has an active rename.\n" +
                      $"Press 'Confirm' if you'd wish your rename request to start after {renames[0].Expiration}EST\n" +
                      $"Press 'Buy Out' if you wish to override all current rename requests and start yours instead.\n" +
                      $"Buying out the renames will cost you **{buyoutCost}** beer.\n" +
                      $"Press 'Cancel' to cancel this request." +
                      $"\n\n```{table}```";

                await ctx.FollowUpAsync(
                    new DiscordFollowupMessageBuilder()
                    .WithContent(content)
                    .AddComponents(acceptPendingBtn, buyOutBtn, cancelBtn)
                    .AsEphemeral(true));

                _discordClient.ComponentInteractionCreated += async (s, e) =>
                {
                    if (e.User.Id != userId) return;

                    if (e.Id == $"rename_confirm_{uniqueId}")
                    {
                        var startDate = DateTime.UtcNow;
                        var expiration = startDate.AddDays(days);
                        await HandleConfirmRename(ctx, e, user, newName, (int)days, renameCost, affectedUserResponse.Id, requestingUserResponse.Id, startDate, expiration, RenameStatusEnum.Active);
                    }
                    else if (e.Id == $"rename_pending_confirm_{uniqueId}")
                    {
                        var startDate = renames[0].Expiration;
                        var expiration = startDate.Value.AddDays(days);
                        await HandlePendingRename(ctx, e, newName, (int)days, affectedUserResponse.Id, requestingUserResponse.Id, startDate, expiration);
                    }
                    else if (e.Id == $"buyout_confirm_{uniqueId}")
                    {
                        await HandleBuyoutRename(ctx, e, user, newName, (int)days, renameCost, affectedUserResponse.Id, requestingUserResponse.Id, buyoutCost);
                    }
                    else if (e.Id == $"rename_cancel_{uniqueId}")
                    {
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder()
                            .WithContent("Rename cancelled."));
                    }
                };
            }

            _logger.LogInformation("Rename command completed successfully via Discord slash command. UserId: {UserId}, Username: {Username}", userId, username);
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Rename command failed - account not found. UserId: {UserId}", ex.UserId);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("That user does not have an account. I cannot change their name right now.")
                .AsEphemeral(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rename command failed - unexpected error. UserId: {UserId}, Username: {Username}", userId, username);

            await ctx.CreateResponseAsync(
                DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("An error occurred while processing the rename. Please try again later.")
                .AsEphemeral(true));
        }
    }

    private async Task HandleConfirmRename(InteractionContext ctx, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e, DiscordUser user, string newName, int days, int renameCost, ulong affectedUserId, ulong requestingUserId, DateTime startDate, DateTime expiration, RenameStatusEnum status)
    {
        try
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

            var renameStatus = await ctx.Guild.GetMemberAsync(affectedUserId);
            if (renameStatus != null)
            {
                await renameStatus.ModifyAsync(x => x.Nickname = newName);
            }

            var createCommand = CreateRenameCommand.From(new CreateRenameRequestDto
            {
                NewName = newName,
                AffectedUserId = affectedUserId,
                RequestedUserId = requestingUserId,
                Days = days,
                StartDate = startDate,
                Expiration = expiration,
                Status = status
            });

            var createResponse = await _createRenameFacade.Execute(createCommand, CancellationToken.None);

            var affectedUserCommand = GetAccountCommand.From(affectedUserId);
            var affectedUserResponse = await _getAccountFacade.Execute(affectedUserCommand, CancellationToken.None);

            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"`{affectedUserResponse.Username}` has been renamed to `{newName}` for the next {days} day(s). It costed you {renameCost} beer."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to confirm rename. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", affectedUserId, requestingUserId);
            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"I couldn't complete that request for some reason. I didn't take any beer for the attempt. Good effort, though. {ex.Message}"));
        }
    }

    private async Task HandlePendingRename(InteractionContext ctx, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e, string newName, int days, ulong affectedUserId, ulong requestingUserId, DateTime? startDate, DateTime expiration)
    {
        try
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

            var createCommand = CreateRenameCommand.From(new CreateRenameRequestDto
            {
                NewName = newName,
                AffectedUserId = affectedUserId,
                RequestedUserId = requestingUserId,
                Days = days,
                StartDate = startDate,
                Expiration = expiration,
                Status = RenameStatusEnum.Pending
            });

            var createResponse = await _createRenameFacade.Execute(createCommand, CancellationToken.None);

            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Your rename request has been set to start after {startDate}EST."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create pending rename. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", affectedUserId, requestingUserId);
            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Failed to create rename: {ex.Message}"));
        }
    }

    private async Task HandleBuyoutRename(InteractionContext ctx, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e, DiscordUser user, string newName, int days, int renameCost, ulong affectedUserId, ulong requestingUserId, int buyoutCost)
    {
        try
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);

            var buyoutCommand = BuyoutRenamesCommand.From(affectedUserId);
            var buyoutResponse = await _buyoutRenamesFacade.Execute(buyoutCommand, CancellationToken.None);

            var startDate = DateTime.UtcNow;
            var expiration = startDate.AddDays(days);

            var createCommand = CreateRenameCommand.From(new CreateRenameRequestDto
            {
                NewName = newName,
                AffectedUserId = affectedUserId,
                RequestedUserId = requestingUserId,
                Days = days,
                StartDate = startDate,
                Expiration = expiration,
                Status = RenameStatusEnum.Active
            });

            var createResponse = await _createRenameFacade.Execute(createCommand, CancellationToken.None);

            var renameStatus = await ctx.Guild.GetMemberAsync(affectedUserId);
            if (renameStatus != null)
            {
                await renameStatus.ModifyAsync(x => x.Nickname = newName);
            }

            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Renames bought out."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to buyout rename. AffectedUserId: {AffectedUserId}, RequestedUserId: {RequestedUserId}", affectedUserId, requestingUserId);
            await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Failed to buyout renames: {ex.Message}"));
        }
    }

    private static int GenerateUniqueId()
    {
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] data = new byte[4];
            rng.GetBytes(data);
            int uniqueId = BitConverter.ToInt32(data, 0);
            return Math.Abs(uniqueId);
        }
    }
}
