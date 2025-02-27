using DSharpPlus.SlashCommands;

namespace Fitz.Features.Accounts.Update.Discord
{
    public class UpdateAccountDto
    {
        public InteractionContext Context { get; set; }
        public ulong UserId { get; set; }
        public string PropertyName { get; set; }
        public object PropertyValue { get; set; }
    }
} 