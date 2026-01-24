namespace Fitz.Api.Controllers.Bank.GetBalanceWithTransactions.Domain;

public record GetBalanceWithTransactionsCommand(ulong UserId)
{
    public static GetBalanceWithTransactionsCommand From(ulong userId)
    {
        return new GetBalanceWithTransactionsCommand(UserId: userId);
    }
}
