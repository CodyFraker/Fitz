namespace Fitz.Api.Models.Responses
{
    public class VoteResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public int? Choice { get; set; }
        public ulong UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
