using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Models.Requests
{
    public class PollOptionRequest
    {
        [Required]
        public string Answer { get; set; } = string.Empty;

        [Required]
        public string EmojiName { get; set; } = string.Empty;

        public ulong? EmojiId { get; set; }
    }
}
