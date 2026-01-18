namespace Fitz.Api.Models.Responses
{
    public class BalancesResponse
    {
        public List<AccountBalanceResponse> Accounts { get; set; } = new();
        public int TotalCount { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
