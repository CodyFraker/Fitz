namespace Fitz.Core.Discord
{
    using DSharpPlus.CommandsNext;
    using System.Linq;

    public static class DiscordExtensions
    {
        public static void UnregisterCommands<T>(this CommandsNextExtension cNext)
        {
            cNext.UnregisterCommands(cNext
                .RegisteredCommands
                .Where(c => c.Value.Module.ModuleType == typeof(T))
                .Select(c => c.Value)
                .ToArray());
        }
    }
}
