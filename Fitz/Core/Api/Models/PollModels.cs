using System;
using System.Collections.Generic;
using Fitz.Features.Polls.Models;

namespace Fitz.Core.Api.Models
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
    }

    public class PollOptionResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Answer { get; set; } = string.Empty;
        public string EmojiName { get; set; } = string.Empty;
        public ulong? EmojiId { get; set; }
    }

    public class VoteResponse
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public int? Choice { get; set; }
        public ulong UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class CreatePollRequest
    {
        public ulong AccountId { get; set; }
        public ulong MessageId { get; set; }
        public string Question { get; set; } = string.Empty;
        public PollType Type { get; set; }
        public List<PollOptionRequest> Options { get; set; } = new();
    }

    public class PollOptionRequest
    {
        public string Answer { get; set; } = string.Empty;
        public string EmojiName { get; set; } = string.Empty;
        public ulong? EmojiId { get; set; }
    }

    public class EvaluatePollRequest
    {
        public PollStatus Status { get; set; }
    }

    public class AddVoteRequest
    {
        public ulong UserId { get; set; }
        public int OptionId { get; set; }
    }

    public class UpdateVoteRequest
    {
        public ulong UserId { get; set; }
        public int OptionId { get; set; }
    }
}
