namespace Fitz.BackgroundServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using Fitz.BackgroundServices.Models;
    using Fitz.Models;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;

    public class JobManager : IDisposable
    {
        private const int IntervalMs = 300000;

        private readonly FitzContextFactory dbFactory;
        private readonly ActivityManager activityManager;
        private readonly FitzLog bloonLog;
        private readonly IServiceProvider services;
        private Timer timer;

        public JobManager(FitzContextFactory dbFactory, ActivityManager activityManager, FitzLog bloonLog, IServiceProvider services)
        {
            this.dbFactory = dbFactory;
            this.activityManager = activityManager;
            this.bloonLog = bloonLog;
            this.services = services;
        }

        public static void AddJobs(ref IServiceCollection services)
        {
            Type iType = typeof(ITimedJob);

            foreach (Type type in Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => typeof(ITimedJob).IsAssignableFrom(t) && t.GetInterfaces().Contains(typeof(ITimedJob))))
            {
                services.AddSingleton(iType, type);
            }
        }

        public void Start()
        {
            this.timer = new Timer(IntervalMs)
            {
                Enabled = true,
                AutoReset = true,
            };

            this.timer.Elapsed += this.TimerElapsed;

            Log.Information($"JobManager ready");
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.timer.Dispose();
            }
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            List<ITimedJob> timedJobs = new List<ITimedJob>();

            using (FitzContext db = this.dbFactory.Create())
            {
                List<Job> jobs = db.Jobs.ToList();

                foreach (ITimedJob timedJob in this.services.GetServices<ITimedJob>())
                {
                    string jobName = timedJob.GetType().FullName;

                    Job job = jobs.Where(j => j.Name == jobName).FirstOrDefault();

                    if (job == null)
                    {
                        job = new Job()
                        {
                            Name = jobName,
                            LastExecution = DateTime.UnixEpoch.ToUniversalTime(),
                        };

                        db.Jobs.Add(job);
                    }

                    if ((DateTime.UtcNow - job.LastExecution).TotalMinutes >= timedJob.Interval)
                    {
                        timedJobs.Add(timedJob);
                        job.LastExecution = DateTime.UtcNow;
                    }
                }

                await db.SaveChangesAsync().ConfigureAwait(false);
            }

            if (timedJobs.Count > 0)
            {
                await this.activityManager.TrySetActivityAsync($"{timedJobs.Count} job(s)", ActivityType.Watching, true).ConfigureAwait(false);

                Task jobs = Task.WhenAll(timedJobs.Select(t => t.Execute()));

                try
                {
                    await jobs.ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    this.bloonLog.Error($"{jobs.Exception.InnerExceptions.Count} job(s) failed! Check logs");
                    Log.Error(jobs.Exception, "One or more jobs failed.");
                }
            }
        }
    }
}
