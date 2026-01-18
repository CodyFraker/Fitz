namespace Fitz.Api.Models.Responses
{
    public class PollsResponse
    {
        public List<PollResponse> Polls { get; set; } = new();
        public int TotalCount { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }
}
