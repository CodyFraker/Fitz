namespace Fitz.Commands
{
    using System.Threading.Tasks;
    using Fitz.Variables;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;

    /**
     * <summary>
     * Precondition for SBG-Exclusives.
     * </summary>
     */
    public class SBGExclusiveAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild?.Id == Roles.All);
    }
}