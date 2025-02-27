using DSharpPlus.SlashCommands;

namespace Fitz.Features.Polls.Models
{
    /// <summary>
    /// Represents the type of poll
    /// </summary>
    public enum PollType
    {
        /// <summary>
        /// A standard poll with custom options
        /// </summary>
        Standard = 0,

        /// <summary>
        /// A poll with numeric options
        /// </summary>
        Number = 1,

        /// <summary>
        /// A poll with color options
        /// </summary>
        Color = 2,

        /// <summary>
        /// A yes/no poll
        /// </summary>
        YesOrNo = 3,

        /// <summary>
        /// A this-or-that poll with two options
        /// </summary>
        ThisOrThat = 4,

        /// <summary>
        /// A hot take poll with agree/disagree options
        /// </summary>
        HotTake = 5
    }
}