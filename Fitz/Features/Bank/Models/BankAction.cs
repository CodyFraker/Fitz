using DSharpPlus.SlashCommands;

namespace Fitz.Features.Bank.Models
{
    public enum BankAction
    {
        [ChoiceName("Add")]
        Add,

        [ChoiceName("Remove")]
        Remove
    }
}