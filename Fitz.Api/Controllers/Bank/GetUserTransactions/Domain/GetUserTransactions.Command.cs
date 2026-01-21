namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;

public record GetUserTransactionsCommand(ulong UserId, int Skip, int Take)
{
    public static GetUserTransactionsCommand From(ulong userId, int skip, int take)
    {
        return new GetUserTransactionsCommand(UserId: userId, Skip: skip, Take: take);
    }
}
