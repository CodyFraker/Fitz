using Fitz.Core.Services.Features;
using Fitz.Core.Services.Jobs;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Fitz.Core
{
    public class Service : IHostedService
    {
        private readonly FeatureManager featureManager;
        private readonly JobManager jobManager;

        public Service(FeatureManager featureManager, JobManager jobManager)
        {
            this.featureManager = featureManager;
            this.jobManager = jobManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.featureManager.InitializeAsync();

            await this.featureManager.Start();

            this.jobManager.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}