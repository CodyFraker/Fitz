using Fitz.Core.Discord;
using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Commands
{
    public class PurchaseLotteryTicketCommand(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<Result> ExecuteAsync(Account user, int amount)
        {
            try
            {
                var transferToFitzCommand = new TransferToFitzCommand(scopeFactory, botLog);
                await transferToFitzCommand.ExecuteAsync(user.Id, amount, Reason.Lotto);
                return new Result(true, $"Purchased {amount} lottery ticket(s).", user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to purchase {amount} lottery ticket(s) for {user.Username} | {user.Id}");
                return new Result(false, $"Failed to purchase {amount} tickets for {user.Username} | {user.Id}", null);
            }
        }
    }
}
