using DSharpPlus.SlashCommands;

namespace Fitz.Features.Polls.Models
{
    public enum PollType
    {
        [ChoiceName("Number")]
        Number = 1,

        [ChoiceName("Yes Or No")]
        YesOrNo = 2,

        [ChoiceName("Color")]
        Color = 3,

        [ChoiceName("This Or That")]
        ThisOrThat = 4,

        [ChoiceName("Hot Take")]
        HotTake = 5,
    }
}