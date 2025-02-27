using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;

namespace Fitz.Features.Bank.AddBalance.Discord
{
    public class AddBalanceResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Account Account { get; set; }

        public static AddBalanceResponse FromResult(Result result)
        {
            return new AddBalanceResponse
            {
                Success = result.Success,
                Message = result.Message,
                Account = result.Data as Account
            };
        }
    }
}
