using DSharpPlus.SlashCommands;

namespace Fitz.Core.Models
{
    public enum HelpAction
    {
        [ChoiceName("Lottery")]
        Lottery,

        [ChoiceName("Accounts")]
        Account,

        [ChoiceName("Renames")]
        Renames,

        [ChoiceName("HappyHour")]
        HappyHour,
    }
}