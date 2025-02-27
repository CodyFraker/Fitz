using Fitz.Features.Accounts.Models;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Domain
{
    public interface IAccountRepository
    {
        Task<Account> GetAccountAsync(ulong userId);
        Task<bool> UpdateAccountAsync(Account account);
        Task<Account> GetAccountByUserId(ulong userId);
        Task SaveAccount(Account account);
    }
} 