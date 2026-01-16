using DSharpPlus.Entities;
using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Accounts.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts
{
    public sealed class AccountService(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> CreateAccountAsync(DiscordUser user)
        {
            var command = new CreateAccountCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(user);
        }

        public async Task<Result> CreateFitzAccountAsync()
        {
            var command = new CreateFitzAccountCommand(scopeFactory, botLog);
            return await command.ExecuteAsync();
        }

        public async Task<Result> SetSafeBalanceAsync(Account account, int safeBalance)
        {
            var command = new SetSafeBalanceCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, safeBalance);
        }

        public async Task<Result> SetSafeBalanceAsync(ulong userId, int safeBalance)
        {
            var findAccountQuery = new FindAccountQuery(scopeFactory);
            var account = findAccountQuery.Execute(userId);
            return await SetSafeBalanceAsync(account, safeBalance);
        }

        public async Task<Result> SetLotterySubscribe(Account account, bool subscribe)
        {
            var command = new SetLotterySubscribeCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, subscribe);
        }

        public async Task<Result> SetTicketAmountAsync(Account account, int amount)
        {
            var command = new SetTicketAmountCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, amount);
        }

        public async Task<Result> SetFavorabilityAsync(Account account, int newFavorability)
        {
            var command = new SetFavorabilityCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, newFavorability);
        }

        public async Task<Result> SetUsernameAsync(Account account, string username)
        {
            var command = new SetUsernameCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, username);
        }

        public async Task<Result> SetDeactivatedAsync(Account account, bool deactivated)
        {
            var command = new SetDeactivatedCommand(scopeFactory, botLog);
            return await command.ExecuteAsync(account, deactivated);
        }

        public List<Account> QueryAccounts()
        {
            var query = new QueryAccountsQuery(scopeFactory);
            return query.Execute();
        }

        public List<Account> GetLotterySubscribers()
        {
            var query = new GetLotterySubscribersQuery(scopeFactory);
            return query.Execute();
        }

        public Account FindAccount(ulong id)
        {
            var query = new FindAccountQuery(scopeFactory);
            return query.Execute(id);
        }

        public Account FindAccount(DiscordUser user)
        {
            var query = new FindAccountQuery(scopeFactory);
            return query.Execute(user);
        }

        public DSharpPlus.Entities.DiscordEmbed AccountHelpEmbed(DSharpPlus.DiscordClient dClient)
        {
            var query = new GetAccountHelpEmbedQuery();
            return query.Execute(dClient);
        }
    }
}
