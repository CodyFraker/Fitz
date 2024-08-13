using DSharpPlus;
using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Fitz.Features.Bank;
using Hangfire;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.HappyHour
{
    public class HappyHourFeature : Feature
    {
        private readonly DiscordClient dClient;
        private readonly HappyHourJob happyHourJob;
        private BankService bankService;

        public HappyHourFeature(DiscordClient dClient, BankService bankService)
        {
            this.dClient = dClient;
            this.bankService = bankService;
            this.happyHourJob = new HappyHourJob(dClient, bankService);
        }

        public override string Name => "Happy Hour";

        public override string Description => "Double the amount of beer when happy hour is active. (7PM-11PM EST)";

        public override Task Disable()
        {
            RecurringJob.RemoveIfExists("HappyHourJob");
            return base.Disable();
        }

        public override Task Enable()
        {
            RecurringJob.AddOrUpdate("HappyHourJob", () => this.happyHourJob.Execute(), this.happyHourJob.Interval);
            return base.Enable();
        }
    }
}