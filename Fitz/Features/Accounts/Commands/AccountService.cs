using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Features.Accounts.Create.Discord;
using Fitz.Features.Accounts.Create.Domain;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Features.Accounts.Update.Persistence;
using Fitz.Variables.Emojis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Commands
{
    // This file provides backward compatibility for the old AccountService class
    // after the refactoring to a more domain-driven design approach.
    
    public class AccountService
    {
        private readonly CreateAccountService _createAccountService;
        private readonly UpdateAccountService _updateAccountService;
        private readonly UpdateAccountRepository _updateAccountRepository;
        
        public AccountService(CreateAccountService createAccountService, UpdateAccountService updateAccountService, UpdateAccountRepository updateAccountRepository)
        {
            _createAccountService = createAccountService;
            _updateAccountService = updateAccountService;
            _updateAccountRepository = updateAccountRepository;
        }
        
        /// <summary>
        /// Creates a new account for a Discord user
        /// </summary>
        /// <param name="user">The Discord user</param>
        /// <returns>A result object containing the created account</returns>
        public async Task<CreateAccountResponse> CreateAccountAsync(DiscordUser user)
        {
            try
            {
                // Check if account already exists
                var existingAccount = FindAccount(user.Id);
                if (existingAccount != null)
                {
                    return new CreateAccountResponse
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = "Account already exists",
                        Account = existingAccount
                    };
                }
                
                // Create the command
                var command = new CreateAccountCommand
                {
                    Id = user.Id,
                    Username = user.Username,
                    CreatedDate = DateTime.UtcNow
                };
                
                // Build the account model
                var accountModel = _createAccountService.BuildAccount(command);
                
                // Save the account
                var account = new Account
                {
                    Id = accountModel.Id,
                    Username = accountModel.Username,
                    Beer = accountModel.Beer,
                    LifetimeBeer = accountModel.LifetimeBeer,
                    safeBalance = accountModel.SafeBalance,
                    Favorability = accountModel.Favorability,
                    CreatedDate = accountModel.CreatedDate,
                    LastSeenDate = accountModel.LastSeenDate,
                    LastActivityDate = accountModel.LastActivityDate,
                    subscribeToLottery = accountModel.SubscribeToLottery,
                    SubscribeTickets = accountModel.SubscribeTickets,
                    Deactivated = accountModel.Deactivated
                };
                
                // Use the update repository to save the account
                await _updateAccountRepository.UpdateAccountAsync(account);
                
                return new CreateAccountResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Account created successfully",
                    Account = account
                };
            }
            catch (Exception ex)
            {
                return new CreateAccountResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Error creating account: {ex.Message}",
                    Account = null
                };
            }
        }
        
        /// <summary>
        /// Gets an account by ID
        /// </summary>
        /// <param name="id">The Discord user ID</param>
        /// <returns>The account, or null if not found</returns>
        public Account FindAccount(ulong id)
        {
            // Use the update account repository to get the account
            return _updateAccountRepository.GetAccountAsync(id).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Sets the favorability of an account
        /// </summary>
        /// <param name="account">The account to update</param>
        /// <param name="favorability">The new favorability value</param>
        /// <returns>Task</returns>
        public async Task SetFavorabilityAsync(Account account, int favorability)
        {
            account.Favorability = favorability;
            
            // Create an UpdateAccountCommand
            var command = new Fitz.Features.Accounts.Update.Domain.UpdateAccountCommand
            {
                Id = account.Id,
                Favorability = favorability
            };
            
            await _updateAccountService.UpdateAccountAsync(command);
        }
        
        /// <summary>
        /// Gets all accounts that are subscribed to the lottery
        /// </summary>
        /// <returns>List of accounts</returns>
        public List<Account> GetLotterySubscribers()
        {
            // Implementation would go here
            return new List<Account>();
        }
        
        /// <summary>
        /// Creates a help embed for account commands
        /// </summary>
        /// <param name="dClient">The Discord client</param>
        /// <returns>A Discord embed with help information</returns>
        public DiscordEmbed AccountHelpEmbed(DiscordClient dClient)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    IconUrl = DiscordEmoji.FromGuildEmote(dClient, AccountEmojis.Users).Url,
                    Text = "Account Commands",
                },
                Color = new DiscordColor(52, 114, 53),
                Timestamp = DateTime.UtcNow,
                Title = "Account Commands",
                Description = "Here are the available account commands:\n\n" +
                    "**`/signup`** - Create a new account\n" +
                    "**`/settings`** - View and update your account settings\n" +
                    "**`/balance`** - Check your beer balance\n" +
                    "**`/donate`** - Donate beer to another user\n" +
                    "**`/leaderboard`** - View the beer leaderboard"
            };
            
            return embed.Build();
        }
    }
} 