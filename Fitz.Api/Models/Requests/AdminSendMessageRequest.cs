using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class AdminSendMessageRequest
    {
        [Required]
        public ulong ChannelId { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
