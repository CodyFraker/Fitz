namespace Fitz.Api.Models.Responses
{
    public class UserFavorabilityResponse
    {
        public ulong UserId { get; set; }
        public string Username { get; set; }
        public int Beer { get; set; }
        public int BotBeer { get; set; }
        public decimal BeerRatio { get; set; }
        public int Favorability { get; set; }
        public bool CanUseCommands { get; set; }
    }
}
