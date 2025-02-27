using System.Threading.Tasks;
using Fitz.Features.Bank.Models;

namespace Fitz.Features.Bank.AddBalance.Persistance
{
    public interface IAddBalanceRepository
    {
        Task LogTransactionAsync(ulong senderId, ulong recipientId, int amount, TransactionReason reason);
    }
} 