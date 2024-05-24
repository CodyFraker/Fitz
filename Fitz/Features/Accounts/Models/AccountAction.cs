using DSharpPlus.SlashCommands;

namespace Fitz.Features.Accounts.Models
{
    public enum AccountAction
    {
        [ChoiceName("Create")]
        Add,

        [ChoiceName("Remove")]
        Remove
    }
}