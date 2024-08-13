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

        public Service(FeatureManager featureManager)
        {
            this.featureManager = featureManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.featureManager.InitializeAsync();

            await this.featureManager.Start();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}