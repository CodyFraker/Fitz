namespace Fitz.Api.Models.Responses
{
    public class AccountResponse
    {
        public ulong Id { get; set; }
        public string? Username { get; set; }
        public int Beer { get; set; }
        public int LifetimeBeer { get; set; }
        public int SafeBalance { get; set; }
        public int Favorability { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool SubscribeToLottery { get; set; }
        public int SubscribeTickets { get; set; }
        public bool Deactivated { get; set; }
    }
}
