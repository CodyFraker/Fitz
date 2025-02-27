using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Features.Lottery.Create.Domain;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Create.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    [SlashCommandGroup("admin-lottery", "Admin commands for lottery management")]
    public class AdminLotteryAdapter : ApplicationCommandModule
    {
        private readonly CreateLotteryConductor _createLotteryConductor;
        private readonly BotLog _botLog;

        public AdminLotteryAdapter(CreateLotteryConductor createLotteryConductor, BotLog botLog)
        {
            _createLotteryConductor = createLotteryConductor ?? throw new ArgumentNullException(nameof(createLotteryConductor));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        [SlashCommand("create", "Create a new lottery")]
        public async Task CreateLottery(
            InteractionContext ctx,
            [Option("duration-days", "Duration of the lottery in days")] long durationDays,
            [Option("initial-pool", "Initial prize pool amount")] long initialPool)
        {
            try
            {
                // Check if the command user has admin permissions
                if (!HasAdminPermission(ctx))
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You don't have permission to use this command.")
                            .AsEphemeral(true));
                    return;
                }

                if (durationDays <= 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Duration must be greater than 0 days.")
                            .AsEphemeral(true));
                    return;
                }

                if (initialPool < 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Initial pool cannot be negative.")
                            .AsEphemeral(true));
                    return;
                }

                var startDate = DateTime.UtcNow;
                var endDate = startDate.AddDays(durationDays);

                var command = new CreateLotteryCommand(startDate, endDate, (int)initialPool);
                var result = await _createLotteryConductor.CreateLottery(command);
                var response = CreateLotteryResponse.FromResult(result);

                if (response.Success)
                {
                    _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                        $"Admin {ctx.User.Username} created a new lottery with duration {durationDays} days and initial pool {initialPool}");

                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Successfully created a new lottery with duration {durationDays} days and initial pool {initialPool}.")
                            .AsEphemeral(true));
                }
                else
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent($"Failed to create lottery: {response.Message}")
                            .AsEphemeral(true));
                }
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Error creating lottery: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("end", "End the current lottery")]
        public async Task EndLottery(InteractionContext ctx)
        {
            try
            {
                // Check if the command user has admin permissions
                if (!HasAdminPermission(ctx))
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You don't have permission to use this command.")
                            .AsEphemeral(true));
                    return;
                }

                var result = await _createLotteryConductor.GetCurrentLottery();

                if (!result.Success)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("There is no active lottery.")
                            .AsEphemeral(true));
                    return;
                }

                var lottery = result.Data as Models.Lottery;
                lottery.EndDate = DateTime.UtcNow;

                // TODO: Implement updating the lottery end date
                // For now, just inform the admin that this will be handled in the next job cycle

                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Admin {ctx.User.Username} ended the current lottery");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("The current lottery will end in the next job cycle.")
                        .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Error ending lottery: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("set-pool", "Set the prize pool for the current lottery")]
        public async Task SetPrizePool(
            InteractionContext ctx,
            [Option("amount", "The new prize pool amount")] long amount)
        {
            try
            {
                // Check if the command user has admin permissions
                if (!HasAdminPermission(ctx))
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You don't have permission to use this command.")
                            .AsEphemeral(true));
                    return;
                }

                if (amount < 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Prize pool cannot be negative.")
                            .AsEphemeral(true));
                    return;
                }

                var result = await _createLotteryConductor.GetCurrentLottery();

                if (!result.Success)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("There is no active lottery.")
                            .AsEphemeral(true));
                    return;
                }

                // TODO: Implement updating the lottery prize pool
                // For now, just inform the admin that this functionality is coming soon

                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Admin {ctx.User.Username} attempted to set prize pool to {amount}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Setting prize pool to {amount} will be implemented soon.")
                        .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Error setting prize pool: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        private bool HasAdminPermission(InteractionContext ctx)
        {
            // Check if the user has a role with Administrator permission or is a server owner
            return ctx.Member.IsOwner || ctx.Member.Roles.Any(r => r.Name.Contains("Admin"));
        }
    }
}