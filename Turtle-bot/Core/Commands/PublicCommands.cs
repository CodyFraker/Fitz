namespace Fitz.Core.Commands
{
    using System.Threading.Tasks;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Fitz.Core.Contexts;

    /// <summary>
    /// This class pertains to the commands that can be ran practically anywhere. They're typically short and simple commands to run.
    /// </summary>
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class PublicCommands : BaseCommandModule
    {
        private readonly BotContext db;

        public PublicCommands(BotContext db)
        {
            this.db = db;
        }

        //[Command("ping")]
        //[Description("This command is to be used when you think the bot is frozen or stuck. It'll reply with **pong**")]
        //public Task PingPongAsync(CommandContext ctx) => ctx.RespondAsync($"pong! Latency: {ctx.Client.Ping}ms");

        [Command("dev")]
        [Description("Shows the invite link to my personal server")]
        public Task DevelopmentURLAsync(CommandContext ctx) => ctx.RespondAsync("https://discord.gg/tAVydGr");
    }
}