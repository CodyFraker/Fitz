using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class UpdateFavorabilitySettingsRequest
    {
        [Range(0.1, 100, ErrorMessage = "Beer ratio threshold must be between 0.1 and 100")]
        public decimal? FavorabilityBeerRatioThreshold { get; set; }

        [Range(0, 100, ErrorMessage = "Low threshold must be between 0 and 100")]
        public int? FavorabilityLowThreshold { get; set; }

        [Range(0, 100, ErrorMessage = "Base drop percent must be between 0 and 100")]
        public decimal? FavorabilityBaseDropPercent { get; set; }

        [Range(0.1, 10, ErrorMessage = "Drop multiplier must be between 0.1 and 10")]
        public decimal? FavorabilityDropMultiplier { get; set; }
    }
}
