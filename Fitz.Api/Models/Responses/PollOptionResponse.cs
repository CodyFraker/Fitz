namespace Fitz.Api.Models.Responses
{
    public class PollOptionResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Answer { get; set; } = string.Empty;
        public string EmojiName { get; set; } = string.Empty;
        public ulong? EmojiId { get; set; }
    }
}
