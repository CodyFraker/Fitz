using Fitz.Database.Entities;

namespace Fitz.Api.Models.Responses
{
    public class PollResponse
    {
        public int Id { get; set; }
        public ulong AccountId { get; set; }
        public ulong MessageId { get; set; }
        public string Question { get; set; } = string.Empty;
        public PollType Type { get; set; }
        public PollStatus Status { get; set; }
        public DateTime? EvaluatedOn { get; set; }
        public DateTime SubmittedOn { get; set; }
        public List<PollOptionResponse>? Options { get; set; }
        public int TotalVotes { get; set; }
        public Dictionary<int, int>? OptionVoteCounts { get; set; }
    }
}
