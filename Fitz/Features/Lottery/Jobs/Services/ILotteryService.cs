using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fitz.Features.Lottery.Models;

namespace Fitz.Features.Lottery.Jobs.Services
{
    public interface ILotteryService
    {
        Models.Lottery GetCurrentLottery();
        Task<List<Winner>> DecideWinners(Models.Lottery lottery);
        Task EndLotteryAsync(Models.Lottery lottery);
        Task StartNewLotteryAsync(DateTime startDate, DateTime endDate, decimal basePool);
        ServiceResponse GetTotalTickets();
        ServiceResponse GetTotalLotteryParticipant();
        ServiceResponse GetRemainingHoursUntilNextDrawing();
        string GetLastWinningTicket();
        List<Account> GetLastLotteryWinnerAccounts();
        ServiceResponse GetUserTickets(Account account);
        Task CreateTicket(Account account, int ticketCount);
        Task AddToPool(int ticketCount);
        DiscordEmbed WinnerEmbed(DiscordClient client, Models.Lottery lottery, List<Account> winners, ulong userId);
    }
}