namespace Fitz.Api.Controllers.Bank.GetBalances.Domain;

public record GetBalancesCommand(int Skip, int Take)
{
    public static GetBalancesCommand From(int skip, int take)
    {
        return new GetBalancesCommand(Skip: skip, Take: take);
    }
}
