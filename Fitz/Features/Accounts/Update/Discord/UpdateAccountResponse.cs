using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;

namespace Fitz.Features.Accounts.Update.Discord
{
    public class UpdateAccountResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Account Account { get; set; }

        public static UpdateAccountResponse FromResult(Result result)
        {
            return new UpdateAccountResponse
            {
                Success = result.Success,
                Message = result.Message,
                Account = result.Data as Account
            };
        }
    }
} 