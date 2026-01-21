namespace Fitz.Api.Controllers.Bank.GetTransactions.Domain;

public record GetTransactionsCommand(int Take)
{
    public static GetTransactionsCommand From(int take)
    {
        return new GetTransactionsCommand(Take: take);
    }
}
