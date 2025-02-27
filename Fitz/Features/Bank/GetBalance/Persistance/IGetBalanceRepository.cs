using System.Collections.Generic;
using System.Threading.Tasks;
using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.GetBalance.Persistance
{
    public interface IGetBalanceRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsAsync(ulong userId, int count);
        Task<IEnumerable<(ulong UserId, string Username, int Balance)>> GetTopBalancesAsync(int count);
    }
} 