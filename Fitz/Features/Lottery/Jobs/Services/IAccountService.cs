using Fitz.Features.Accounts.Models;
using System.Collections.Generic;

namespace Fitz.Features.Lottery.Jobs.Services
{
    public interface IAccountService
    {
        List<Account> GetLotterySubscribers();
    }
}