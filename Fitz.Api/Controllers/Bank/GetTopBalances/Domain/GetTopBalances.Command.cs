namespace Fitz.Api.Controllers.Bank.GetTopBalances.Domain;

public record GetTopBalancesCommand(int Limit)
{
    public static GetTopBalancesCommand From(int limit)
    {
        return new GetTopBalancesCommand(Limit: limit);
    }
}
