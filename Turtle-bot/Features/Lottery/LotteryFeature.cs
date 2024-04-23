using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using Fitz.Core.Discord;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery
{
    public class LotteryFeature : Feature
    {
        private readonly JobManager jobManager;
        private readonly LotteryJob lottoJob;
        private readonly SlashCommandsExtension slash;
        private readonly CommandsNextExtension cNext;

        public LotteryFeature(JobManager jobManager, LotteryJob lotteryJob, DiscordClient dClient, LotteryService lotteryService)
        {
            this.jobManager = jobManager;
            this.lottoJob = new LotteryJob(dClient, lotteryService);
            this.slash = dClient.GetSlashCommands();
            this.cNext = dClient.GetCommandsNext();
        }

        public override string Name => "Lottery";

        public override string Description => "The great beer loterry";

        public override Task Disable()
        {
            // WE CANNOT UNREGISTER SLASH COMMANDS.
            this.jobManager.RemoveJob(this.lottoJob);
            this.cNext.UnregisterCommands<LotteryAdminCommands>();
            return base.Disable();
        }

        public override Task Enable()
        {
            this.slash.RegisterCommands<LotterySlashCommands>();
            this.cNext.RegisterCommands<LotteryAdminCommands>();
            this.jobManager.AddJob(this.lottoJob);
            return base.Enable();
        }
    }
}