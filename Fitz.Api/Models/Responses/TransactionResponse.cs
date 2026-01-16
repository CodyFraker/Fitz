namespace Fitz.Api.Models.Responses
{
    public class TransactionResponse
    {
        public int Id { get; set; }
        public ulong Sender { get; set; }
        public ulong Recipient { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
