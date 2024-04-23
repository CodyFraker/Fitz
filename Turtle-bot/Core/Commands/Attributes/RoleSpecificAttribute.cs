//namespace Fitz.Core.Commands.Attributes
//{
//    using System.Threading.Tasks;
//    using Bot.Variables;
//    using DSharpPlus.CommandsNext;
//    using DSharpPlus.CommandsNext.Attributes;

//    /**
//     * <summary>
//     * Precondition for SBG-Exclusives.
//     * </summary>
//     */
//    public class SBGExclusiveAttribute : CheckBaseAttribute
//    {
//        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) => Task.FromResult(ctx.Guild?.Id == Roles.exampleRole);
//    }
//}