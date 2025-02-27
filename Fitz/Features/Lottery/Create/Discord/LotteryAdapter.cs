using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Features.Lottery.Create.Domain;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Create.Discord
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    [SlashCommandGroup("lottery", "Commands for lottery participation")]
    public class LotteryAdapter : ApplicationCommandModule
    {
        private readonly CreateLotteryConductor _createLotteryConductor;
        private readonly BotLog _botLog;

        public LotteryAdapter(CreateLotteryConductor createLotteryConductor, BotLog botLog)
        {
            _createLotteryConductor = createLotteryConductor ?? throw new ArgumentNullException(nameof(createLotteryConductor));
            _botLog = botLog ?? throw new ArgumentNullException(nameof(botLog));
        }

        [SlashCommand("info", "Get information about the current lottery")]
        public async Task GetLotteryInfo(InteractionContext ctx)
        {
            try
            {
                var result = await _createLotteryConductor.GetCurrentLottery();

                if (!result.Success)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("There is no active lottery at the moment.")
                            .AsEphemeral(true));
                    return;
                }

                var lottery = result.Data as Models.Lottery;

                var embed = new DiscordEmbedBuilder()
                    .WithTitle("🎟️ Current Lottery Information")
                    .WithColor(DiscordColor.Gold)
                    .WithDescription("Here's the information about the current lottery:")
                    .AddField("Start Date", lottery.StartDate.ToString("yyyy-MM-dd HH:mm:ss UTC"), true)
                    .AddField("End Date", lottery.EndDate.ToString("yyyy-MM-dd HH:mm:ss UTC"), true)
                    .AddField("Prize Pool", $"{lottery.Pool} coins", true)
                    .AddField("Time Remaining", GetTimeRemaining(lottery.EndDate), false)
                    .WithFooter("Use /lottery buy to purchase tickets!");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder().AddEmbed(embed));

                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"User {ctx.User.Username} requested lottery information");
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Error getting lottery info: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("buy", "Buy lottery tickets")]
        public async Task BuyTickets(
            InteractionContext ctx,
            [Option("quantity", "Number of tickets to buy")] long quantity = 1)
        {
            try
            {
                if (quantity <= 0)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You must buy at least 1 ticket.")
                            .AsEphemeral(true));
                    return;
                }

                if (quantity > 100)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("You can buy a maximum of 100 tickets at once.")
                            .AsEphemeral(true));
                    return;
                }

                var result = await _createLotteryConductor.GetCurrentLottery();

                if (!result.Success)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("There is no active lottery at the moment.")
                            .AsEphemeral(true));
                    return;
                }

                // TODO: Implement ticket purchase functionality
                // For now, just inform the user that this functionality is coming soon

                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"User {ctx.User.Username} attempted to buy {quantity} tickets");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"Buying tickets will be implemented soon. You requested {quantity} tickets.")
                        .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Error buying tickets: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        [SlashCommand("my-tickets", "View your lottery tickets")]
        public async Task ViewMyTickets(InteractionContext ctx)
        {
            try
            {
                var result = await _createLotteryConductor.GetCurrentLottery();

                if (!result.Success)
                {
                    await ctx.CreateResponseAsync(
                        DiscordInteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("There is no active lottery at the moment.")
                            .AsEphemeral(true));
                    return;
                }

                // TODO: Implement ticket viewing functionality
                // For now, just inform the user that this functionality is coming soon

                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"User {ctx.User.Username} attempted to view their tickets");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("Viewing your tickets will be implemented soon.")
                        .AsEphemeral(true));
            }
            catch (Exception ex)
            {
                _botLog.Information(LogConsoleSettings.LotteryLog, LotteryEmojis.Lottery,
                    $"Error viewing tickets: {ex.Message}");

                await ctx.CreateResponseAsync(
                    DiscordInteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent($"An error occurred: {ex.Message}")
                        .AsEphemeral(true));
            }
        }

        private string GetTimeRemaining(DateTime endDate)
        {
            var timeSpan = endDate - DateTime.UtcNow;

            if (timeSpan.TotalSeconds <= 0)
                return "Lottery has ended";

            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} days, {timeSpan.Hours} hours, {timeSpan.Minutes} minutes";

            if (timeSpan.TotalHours >= 1)
                return $"{timeSpan.Hours} hours, {timeSpan.Minutes} minutes";

            return $"{timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds";
        }
    }
}