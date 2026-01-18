namespace Fitz.Api.Models.Responses
{
    public class SettingsResponse
    {
        public int PollSubmittedPenalty { get; set; }
        public int PollDeclinedPenalty { get; set; }
        public int MaxPendingPolls { get; set; }
        public decimal FavorabilityBeerRatioThreshold { get; set; }
        public int FavorabilityLowThreshold { get; set; }
        public decimal FavorabilityBaseDropPercent { get; set; }
        public decimal FavorabilityDropMultiplier { get; set; }
    }
}
