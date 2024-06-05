using DSharpPlus.SlashCommands;

namespace Fitz.Features.Polls.Models
{
    public enum PollStatus
    {
        [ChoiceName("Pending")]
        Pending = 1,

        [ChoiceName("Approved")]
        Approved = 2,

        [ChoiceName("Declined")]
        Declined = 3
    }
}