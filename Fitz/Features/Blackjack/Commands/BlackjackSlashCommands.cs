using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Fitz.Core.Commands.Attributes;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Fitz.Features.Blackjack.Modals;
using Fitz.Variables;
using Fitz.Variables.Emojis;
using QRCoder.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Blackjack.Commands
{
    [SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class BlackjackSlashCommands : ApplicationCommandModule
    {
        private readonly DiscordClient dClient;
        private readonly AccountService accountService;
        private readonly BankService bankService;
        public List<Player> Players = new List<Player>();
        public DiscordMessage BlackjackMessage;

        public BlackjackSlashCommands(DiscordClient dClient, AccountService accountService, BankService bankService)
        {
            Account fitzDealer = accountService.FindAccount(Users.Fitz);
            // Star game with Fitz as the dealer
            BlackjackGame game = new BlackjackGame(fitzDealer, Players);
            game.Play(balance: 500, bet: 5);
            this.dClient = dClient;
            this.accountService = accountService;
            this.bankService = bankService;

            this.dClient.ComponentInteractionCreated += async (s, e) =>
            {
                // If the confirm button was pressed
                if (e.Id == "blackjack_join")
                {
                    if (Players.Exists(x => x.Account.Id == e.User.Id))
                    {
                        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are already in the game.").AsEphemeral(true));
                        return;
                    }
                    else
                    {
                        Account newPlayer = accountService.FindAccount(e.User.Id);
                        Players.Add(new Player()
                        {
                            Account = newPlayer,
                        });
                        await e.Interaction.DeferAsync(true);
                        await e.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                            .WithContent("You have joined the game.").AsEphemeral(true));
                    }
                    DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
                    DiscordButtonComponent startBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_Start", "Start", false);

                    await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder()
                                               .ClearEmbeds().AddEmbed(blackJackEmbed(Players)).AddComponents(startBtn, joinBtn));
                }
                if (e.Id == "blackjack_player_ready")
                {
                    Player player = Players.Find(x => x.Account.Id == e.User.Id);

                    DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
                    DiscordButtonComponent startBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_Start", "Start", false);

                    await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder()
                           .ClearEmbeds().AddEmbed(blackJackEmbed(Players)).AddComponents(startBtn, joinBtn));
                }
                // if the cancel button was pressed
                if (e.Id == "blackjack_leave")
                {
                    await e.Channel.SendMessageAsync("MOO ON THE WAY OUT, YOU COWARD");
                }

                if (e.Id == "blackjack_Start")
                {
                    if ((game.AllowedActions & GameAction.Deal) == GameAction.Deal)
                    {
                        game.Deal();
                        game.Players.FirstOrDefault().Turn = true;
                    }
                    DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_hit", "Hit", true);
                    DiscordButtonComponent stayBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "blackjack_stay", "Stay", true);
                    DiscordButtonComponent leaveBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_leave", "Quit", false);

                    if ((game.AllowedActions & GameAction.Hit) == GameAction.Hit)
                    {
                        hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_hit", "Hit", false);
                    }
                    if ((game.AllowedActions & GameAction.Stand) == GameAction.Stand)
                    {
                        stayBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "blackjack_stay", "Stay", false);
                    }
                    await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).AddComponents(hitBtn, stayBtn, leaveBtn));
                }

                if (e.Id == "blackjack_hit")
                {
                    if (isPlayerInGame(e.User.Id))
                    {
                        if (game.Players.Where(x => x.Account.Id == e.User.Id).FirstOrDefault().Turn == true)
                        {
                            game.Hit(Players.Where(x => x.Account.Id == e.User.Id).FirstOrDefault());
                            DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_hit", "Hit", true);
                            DiscordButtonComponent stayBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "blackjack_stay", "Stay", true);
                            DiscordButtonComponent leaveBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_leave", "Quit", false);

                            if ((game.AllowedActions & GameAction.Hit) == GameAction.Hit)
                            {
                                hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_hit", "Hit", false);
                            }
                            if ((game.AllowedActions & GameAction.Stand) == GameAction.Stand)
                            {
                                stayBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "blackjack_stay", "Stay", false);
                            }
                            if (game.LastState == GameState.DealerWon)
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).WithContent("Dealer Won."));
                                Players.Clear();
                                return;
                            }
                            else if (game.LastState == GameState.PlayerWon)
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).WithContent("Player won."));
                                Players.Clear();
                                return;
                            }
                            else if (game.LastState == GameState.Draw)
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).WithContent("Game Draw."));
                                Players.Clear();
                                return;
                            }
                            else
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).AddComponents(hitBtn, stayBtn, leaveBtn));
                                return;
                            }
                        }
                        else
                        {
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .WithContent("It's not your turn yet.").AsEphemeral(true));
                        }
                    }
                }

                if (e.Id == "blackjack_stay")
                {
                    if (isPlayerInGame(e.User.Id))
                    {
                        if (game.Players.Where(x => x.Account.Id == e.User.Id).FirstOrDefault().Turn == true)
                        {
                            game.Stand(Players.Where(x => x.Account.Id == e.User.Id).FirstOrDefault());
                            DiscordButtonComponent hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_hit", "Hit", true);
                            DiscordButtonComponent stayBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "blackjack_stay", "Stay", true);
                            DiscordButtonComponent leaveBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_leave", "Quit", false);

                            if ((game.AllowedActions & GameAction.Hit) == GameAction.Hit)
                            {
                                hitBtn = new DiscordButtonComponent(DiscordButtonStyle.Primary, "blackjack_hit", "Hit", false);
                            }
                            if ((game.AllowedActions & GameAction.Stand) == GameAction.Stand)
                            {
                                stayBtn = new DiscordButtonComponent(DiscordButtonStyle.Secondary, "blackjack_stay", "Stay", false);
                            }

                            if (game.LastState == GameState.DealerWon)
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).WithContent("Dealer Won."));
                                Players.Clear();
                            }
                            else if (game.LastState == GameState.PlayerWon)
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).WithContent("Player won."));
                                Players.Clear();
                            }
                            else if (game.LastState == GameState.Draw)
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).WithContent("Game Draw."));
                                Players.Clear();
                            }
                            else
                            {
                                await BlackjackMessage.ModifyAsync(new DiscordMessageBuilder().ClearEmbeds().AddEmbed(blackJackEmbed(game)).AddComponents(hitBtn, stayBtn, leaveBtn));
                            }
                        }
                        else
                        {
                            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                .WithContent("It's not your turn yet.").AsEphemeral(true));
                        }
                    }
                }
            };
        }

        [SlashCommand("blackjack", "Start a game of blackjack")]
        [RequireAccount]
        public async Task BlackJack(InteractionContext ctx)
        {
            DiscordButtonComponent joinBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, "blackjack_join", "Join", false);
            DiscordButtonComponent leaveBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, "blackjack_leave", "Leave", false);

            DiscordChannel discordChannel = ctx.Channel;

            BlackjackMessage = await discordChannel.SendMessageAsync(new DiscordMessageBuilder()
                .AddEmbed(blackJackEmbed(Players))
                .AddComponents(joinBtn, leaveBtn));

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Starting blackjack game...").AsEphemeral(true));

            return;
        }

        private DiscordEmbed blackJackEmbed(List<Player> Players)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, LotteryEmojis.Lottery).Url,
                    Text = $"Blackjack #1 | Last Winner: Fitz",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"Starting new blackjack game",
                Description = $"Waiting for others to join..\n",
            };

            string playerNames = string.Empty;
            foreach (Player player in Players)
            {
                playerNames += $"{player.Account.Username} |{DiscordEmoji.FromGuildEmote(dClient, PollEmojis.Yes)}\n";
            }

            if (playerNames == string.Empty)
            {
                playerNames = "No players have joined yet.";
            }

            embed.AddField($"Players", playerNames);

            return embed.Build();
        }

        private DiscordEmbed blackJackEmbed(BlackjackGame game)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, BlackjackEmojis.Cards).Url,
                    Text = $"Blackjack #1 | Last Winner: ???",
                },
                Color = new DiscordColor(52, 114, 53),
                Title = $"Blackjack",
                Description = $"Dealer: {game.Dealer.Fitz.Username}\n" +
                $"Beer Pool: 128\n",
            };

            string dealerHand = string.Empty;
            foreach (Card card in game.Dealer.Hand.Cards)
            {
                switch (card.Suite)
                {
                    case Suite.Spades:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":spades:")}\n";
                        break;

                    case Suite.Diamond:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":diamonds:")}\n";
                        break;

                    case Suite.Club:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":clubs:")}\n";
                        break;

                    case Suite.Heart:
                        dealerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":heart:")}\n";
                        break;
                }
            }

            if (game.LastState == GameState.DealerWon)
            {
                embed.AddField($"{DiscordEmoji.FromName(dClient, ":house:")}Dealer Hand: {game.Dealer.Hand.TotalValue}{DiscordEmoji.FromName(dClient, ":star:")}", dealerHand);
            }
            else
            {
                if (game.LastState == GameState.PlayerWon)
                {
                    embed.AddField($"{DiscordEmoji.FromName(dClient, ":house:")}~~Dealer Hand: {game.Dealer.Hand.TotalValue}~~", dealerHand);
                }
                else
                {
                    embed.AddField($"{DiscordEmoji.FromName(dClient, ":house:")}Dealer Hand: {game.Dealer.Hand.TotalValue}", dealerHand);
                }
            }

            foreach (Player player in game.Players)
            {
                string playerHand = string.Empty;
                foreach (Card card in player.Hand.Cards)
                {
                    switch (card.Suite)
                    {
                        case Suite.Spades:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":spades:")}\n";
                            break;

                        case Suite.Diamond:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":diamonds:")}\n";
                            break;

                        case Suite.Club:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":clubs:")}\n";
                            break;

                        case Suite.Heart:
                            playerHand += $"{card.Rank}{DiscordEmoji.FromName(dClient, ":heart:")}\n";
                            break;
                    }
                }
                if (player.Lost)
                {
                    embed.AddField($"~~{player.Account.Username}'s Hand: {player.Hand.TotalValue}~~{DiscordEmoji.FromName(dClient, ":x:")}", playerHand);
                }
                else
                {
                    if (player.Turn)
                    {
                        embed.AddField($"{DiscordEmoji.FromName(dClient, ":arrow_right:")}{player.Account.Username}'s Hand: {player.Hand.TotalValue}", playerHand);
                    }
                    else
                    {
                        if (!player.Lost && game.LastState == GameState.PlayerWon)
                        {
                            embed.AddField($"{player.Account.Username}'s Hand: {player.Hand.TotalValue}{DiscordEmoji.FromName(dClient, ":star:")}", playerHand);
                        }
                        else
                        {
                            embed.AddField($"{player.Account.Username}'s Hand: {player.Hand.TotalValue}", playerHand);
                        }
                    }
                }
            }

            return embed.Build();
        }

        private bool isPlayerInGame(ulong id)
        {
            return Players.Exists(x => x.Account.Id == id);
        }
    }
}