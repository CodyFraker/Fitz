using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class ExchangeTokenRequest
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string RedirectUri { get; set; } = string.Empty;
    }
}
