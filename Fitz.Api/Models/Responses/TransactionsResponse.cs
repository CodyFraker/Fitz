namespace Fitz.Api.Models.Responses
{
    public class TransactionsResponse
    {
        public List<TransactionResponse> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
