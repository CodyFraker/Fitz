namespace Fitz.Api.Controllers.Account.Exceptions
{
    public class AccountNotFound(ulong userId) 
        : Exception($"Account not found for user ID: {userId}")
    {
        public readonly ulong UserId = userId;
    }
}
