using DSharpPlus.Entities;
using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Api.Controllers.Account.GetAccount.Domain;
using Fitz.Core.Commands;
using Fitz.Database.Entities;
using Fitz.Variables;
using Fitz.Variables.Emojis;

namespace Fitz.Api.Controllers.Account.Embeds
{
    public record AccountEmbed(DiscordUser DiscordUser, AccountEntity account)
    {
        private static readonly DiscordColor EmbedColor = new(52, 114, 53);
        public static DiscordEmbed FromCreateAccount(DiscordUser discordUser, CreateAccountResponse createAccountResponse)
        {
            string subscribe = createAccountResponse.SubscribedToLottery ? "Active" : "Inactive";
            DiscordEmbedBuilder accountEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    //IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, AccountEmojis.Users).Url,
                    Text = $"Account Information",
                },
                Color = EmbedColor,
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = discordUser.AvatarUrl,
                },
                Description = "I collect beer and stupid user data.\n" +
                $"Edit your account settings using `/settings`\n\n" +
                $"**Beer**: `{createAccountResponse.Beer}`\n" +
                $"**Lifetime Beer**: `{createAccountResponse.LifetimeBeer}`\n" +
                $"**Favorability**: `{createAccountResponse.Favorability}%`\n" +
                $"**Lottery Subscription**: `{subscribe}`\n" +
                $"**Safe Balance**: `{createAccountResponse.SafeBalance}`"
            };

            //accountEmbed.AddField($"**Polls**", $"Submitted: `{userPolls.Count()}`\n" +
            //    $"Approved: `{userPolls.Where(poll => poll.Status == PollStatus.Approved).Count()}`\n" +
            //    $"Pending: `{userPolls.Where(poll => poll.Status == PollStatus.Pending).Count()}`\n" +
            //    $"Declined: `{userPolls.Where(poll => poll.Status == PollStatus.Declined).Count()}`", true);

            //accountEmbed.AddField($"**Lottery**", $"Partcipated: `{this.lotteryService.GetTotalLotteryPartipationsByUserId(account.Id)}`\n" +
            //    $"Lifetime Entries: `{userTickets.Count()}`\n" +
            //    $"Wins: `{this.lotteryService.GetTotalWinsByAccountId(account.Id)}`\n" +
            //    $"Largest Payout: `{this.lotteryService.GetLargestPayoutByUserId(account.Id)}`", true);

            //accountEmbed.AddField($"**Renames**", $"Requests: `{this.renameService.GetTotalRenameRequestsByAccountId(account.Id)}`\n" +
            //    $"Renamed: `{this.renameService.GetTotalRenamesByAccountId(account.Id)}`\n" +
            //    $"Highest Cost: `WIP`\n", true);

            return accountEmbed.Build();
        }

        public static DiscordEmbed FromGetAccount(DiscordUser discordUser, GetAccountResponse getAccountResponse)
        {
            string subscribe = getAccountResponse.SubscribeToLottery ? "Active" : "Inactive";
            DiscordEmbedBuilder accountEmbed = new()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    //IconUrl = DiscordEmoji.FromGuildEmote(this.dClient, AccountEmojis.Users).Url,
                    Text = $"Account Information",
                },
                Color = EmbedColor,
                Timestamp = DateTime.UtcNow,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = discordUser.AvatarUrl,
                },
                Description = "I collect beer and stupid user data.\n" +
                $"Edit your account settings using `/settings`\n\n" +
                $"**Beer**: `{getAccountResponse.Beer}`\n" +
                $"**Lifetime Beer**: `{getAccountResponse.LifetimeBeer}`\n" +
                $"**Favorability**: `{getAccountResponse.Favorability}%`\n" +
                $"**Lottery Subscription**: `{subscribe}`\n" +
                $"**Safe Balance**: `{getAccountResponse.SafeBalance}`"
            };

            return accountEmbed.Build();
        }
    }
}
