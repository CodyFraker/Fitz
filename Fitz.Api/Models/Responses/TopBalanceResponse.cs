namespace Fitz.Api.Models.Responses
{
    public class TopBalanceResponse
    {
        public List<AccountBalanceResponse> Accounts { get; set; } = new();
    }

    public class AccountBalanceResponse
    {
        public ulong Id { get; set; }
        public string? Username { get; set; }
        public int Beer { get; set; }
    }
}
