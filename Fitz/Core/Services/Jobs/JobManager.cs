namespace Fitz.Core.Services.Jobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Timers;
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Fitz.Core.Contexts;
    using Fitz.Core.Discord;
    using Fitz.Utils;

    public class JobManager : IDisposable
    {
        // 300,000 = 5 minutes. 60,000 = 1 minute. Useful for debugging Jobs
        private const int IntervalMs = 60000;

        private readonly BotLog botLog;
        private readonly IServiceScopeFactory factory;
        private readonly HashSet<ITimedJob> jobs;
        private Timer timer;

        public JobManager(IServiceScopeFactory factory, BotLog botLog)
        {
            this.factory = factory;
            this.botLog = botLog;
            jobs = new HashSet<ITimedJob>();
        }

        public void Start()
        {
            timer = new Timer(IntervalMs)
            {
                Enabled = true,
                AutoReset = true,
            };

            timer.Elapsed += TimerElapsed;
        }

        public void AddJob(ITimedJob job)
        {
            jobs.Add(job);
        }

        public void RemoveJob(ITimedJob job)
        {
            jobs.Remove(job);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (timer == null)
            {
                return;
            }

            if (disposing)
            {
                timer.Dispose();
            }
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            List<ITimedJob> timedJobs = new List<ITimedJob>();
            using IServiceScope scope = factory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            List<Job> dbJobs = db.Jobs.ToList();

            foreach (ITimedJob timedJob in jobs)
            {
                string jobName = timedJob.GetType().FullName!;

                Job job = dbJobs.Where(j => j.Name == jobName).FirstOrDefault();

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

            await db.SaveChangesAsync();

            if (timedJobs.Count > 0)
            {
                Task jobs = Task.WhenAll(timedJobs.Select(t => t.Execute()));

                try
                {
                    await jobs;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    if (jobs.Exception != null)
                    {
                        for (int i = 0; i < jobs.Exception.InnerExceptions.Count; i++)
                        {
                            botLog.Error($"{jobs.Exception.InnerExceptions[i].ToString().Truncate(1500)}");
                        }
                    }
                    else
                    {
                        botLog.Error($"{ex.ToString().Truncate(1500)}");
                    }

                    Log.Error(jobs.Exception, "One or more jobs failed.");
                }

                // this.botLog.Information(LogConsole.Jobs, SBGEmojis.Bloon, $"Jobs finished:\n- {string.Join("\n- ", timedJobs.Select(j => $"{DiscordEmoji.FromGuildEmote(this.dClient, j.Emoji)} {j.GetType().Name}"))}");
            }
        }
    }
}