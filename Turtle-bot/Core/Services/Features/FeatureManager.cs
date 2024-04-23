using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Fitz.Core.Contexts;

namespace Fitz.Core.Services.Features
{
    public class FeatureManager
    {
        private readonly IServiceProvider provider;
        private readonly List<Feature> features;
        private readonly List<Feature> featuresToEnable;

        public FeatureManager(IServiceProvider provider)
        {
            this.provider = provider;
            this.features = new List<Feature>();
            this.featuresToEnable = new List<Feature>();
        }

        public IReadOnlyList<Feature> Features => this.features
            .Where(f => !f.Protected)
            .ToList()
            .AsReadOnly();

        public async Task InitializeAsync()
        {
            using IServiceScope scope = this.provider.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            List<FeatureStatus> featureStatuses = await db.FeatureStatuses
                .ToListAsync();

            foreach (Type type in Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => typeof(Feature).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract)
                .OrderBy(t => t.Name))
            {
                Feature feature = ActivatorUtilities.CreateInstance(this.provider, type) as Feature;

                if (this.features.Any(f => f.Name == feature.Name))
                {
                    throw new DuplicateNameException($"Duplicate feature name \"{feature.Name}\" for \"{type.Name}\"!");
                }

                FeatureStatus featureStatus = featureStatuses.Where(m => m.Name == feature.Name).FirstOrDefault();

                if (featureStatus == null)
                {
                    featureStatus = new FeatureStatus
                    {
                        Name = feature.Name,
                        Enabled = true,
                    };

                    if (!feature.Protected)
                    {
                        db.FeatureStatuses.Add(featureStatus);
                    }
                }

                Log.Information("Loaded feature {0} - {1}", feature.Name.PadRight(32), featureStatus.Enabled);

                await feature.Initialize();

                if (featureStatus.Enabled)
                {
                    this.featuresToEnable.Add(feature);
                }

                this.features.Add(feature);
            }

            await db.SaveChangesAsync();
        }

        public async Task Start()
        {
            for (int i = 0; i < this.featuresToEnable.Count; i++)
            {
                await this.featuresToEnable[i].Enable();
            }
        }

        public async Task UpdateFeatureStatusAsync(string featureName, bool enabled)
        {
            using IServiceScope scope = this.provider.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            FeatureStatus featureStatus = await db.FeatureStatuses
                .Where(m => m.Name == featureName)
                .FirstOrDefaultAsync();

            if (featureStatus == null)
            {
                featureStatus = new FeatureStatus
                {
                    Name = featureName,
                };

                db.FeatureStatuses.Add(featureStatus);
            }

            featureStatus.Enabled = enabled;

            await db.SaveChangesAsync();
        }
    }
}