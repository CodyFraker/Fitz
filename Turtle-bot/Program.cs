namespace Fitz
{
    using System;
    using System.Net.WebSockets;
    using System.Threading.Tasks;
    using Fitz.BackgroundServices;
    using Fitz.Models;
    using dotenv.net;
    using DSharpPlus;
    using DSharpPlus.EventArgs;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Events;

    /// <summary>
    /// This is the main class which bloon requires in order to go live. Its the brains of the entire setup.
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
    internal class Program
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private DiscordClient dClient;
        private IServiceProvider serviceProvider;

        public static WebSocketState SocketState { get; private set; }

        private static void Main() =>
            new Program().RunAsync().GetAwaiter().GetResult();

        private async Task RunAsync()
        {
            DotEnv.Config();

            // Configure logging
            // Log everything to console
            // Only log warning level and above to file
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .Enrich.FromLogContext()
#if DEBUG
                .WriteTo.FitzConsoleSink(outputTemplate: "[{Timestamp:HH:mm:ss}][{Level:u3}]{Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Debug)
#endif
                .WriteTo.File(
                    "Logs/.log",
                    outputTemplate: "[{Timestamp:HH:mm:ss}][{Level:u3}]{Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            AppDomain.CurrentDomain.ProcessExit += this.OnShutdown;
            AppDomain.CurrentDomain.UnhandledException += this.UnhandledException;

            this.serviceProvider = this.ConfigureServices();

            EventManager.RegisterEventHandlers(this.serviceProvider);

            new CommandHandler(this.serviceProvider).Initialize();

            this.serviceProvider.GetService<JobManager>().Start();

            this.dClient.SocketOpened += this.OnSocketOpened;
            this.dClient.SocketClosed += this.OnSocketClosed;
            this.dClient.SocketErrored += this.OnSocketErrored;
            this.dClient.Ready += this.OnReady;
            await this.dClient.InitializeAsync().ConfigureAwait(false);
            await this.dClient.ConnectAsync().ConfigureAwait(false);

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private Task OnReady(ReadyEventArgs e) => this.serviceProvider.GetService<ActivityManager>().ResetActivityAsync();

        private Task OnSocketClosed(SocketCloseEventArgs e)
        {
            SocketState = WebSocketState.Closed;
            return Task.CompletedTask;
        }

        private Task OnSocketErrored(SocketErrorEventArgs e)
        {
            SocketState = WebSocketState.Closed;
            Log.Error(e.Exception, "Socket errored");
            return Task.CompletedTask;
        }

        private Task OnSocketOpened()
        {
            SocketState = WebSocketState.Open;
            return Task.CompletedTask;
        }

        private IServiceProvider ConfigureServices()
        {
            this.dClient = new DiscordClient(new DiscordConfiguration
            {
#if DEBUG
                LogLevel = LogLevel.Debug,
#endif
                MessageCacheSize = 0,
                Token = Environment.GetEnvironmentVariable("BOT_TOKEN"),
                TokenType = TokenType.Bot,
            });

            this.dClient.DebugLogger.LogMessageReceived += this.ConvertToSerilog;

            IServiceCollection services = new ServiceCollection();

            // DB
            services.AddDbContextPool<FitzContext>(options => options.UseMySql(new FitzContextFactory().ConnectionString));
            services.AddSingleton<FitzContextFactory>();

            // General
            services.AddSingleton(this.dClient)
                .AddSingleton<FitzLog>()
                .AddSingleton<ActivityManager>();

            // Jobs
            services.AddSingleton<JobManager>();
            JobManager.AddJobs(ref services);

            Log.Information("Services configured");

            return services.BuildServiceProvider();
        }

        private void ConvertToSerilog(object sender, DebugLogMessageEventArgs args)
        {
            LogEventLevel level = args.Level switch
            {
                LogLevel.Critical => LogEventLevel.Fatal,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Info => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                _ => LogEventLevel.Debug,
            };

            Log.Write(level, args.Exception, $"[{args.Application}] {args.Message}");
        }

        private void OnShutdown(object sender, EventArgs args)
        {
            this.serviceProvider.GetService<DiscordClient>().Dispose();
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Log.Error("{ExceptionObject}", args.ExceptionObject);
        }
    }
}
