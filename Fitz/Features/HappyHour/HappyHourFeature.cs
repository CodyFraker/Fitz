using DSharpPlus;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Bank;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.HappyHour
{
    public class HappyHourFeature : Feature
    {
        private readonly DiscordClient dClient;
        private readonly JobManager jobManager;
        private readonly HappyHourJob happyHourJob;
        private BankService bankService;

        public HappyHourFeature(DiscordClient dClient, JobManager jobManager, BankService bankService)
        {
            this.dClient = dClient;
            this.jobManager = jobManager;
            this.bankService = bankService;
            this.happyHourJob = new HappyHourJob(dClient, bankService);
        }

        public override string Name => "Happy Hour";

        public override string Description => "Double the amount of beer when happy hour is active. (7PM-11PM EST)";

        public override Task Disable()
        {
            this.jobManager.RemoveJob(this.happyHourJob);
            return base.Disable();
        }

        public override Task Enable()
        {
            this.jobManager.AddJob(this.happyHourJob);
            return base.Enable();
        }
    }
}