namespace Fitz
{
    using dotenv.net;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.SystemConsole.Themes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Fitz.Core.Services;
    using Fitz.Core;
    using Fitz.Metrics;
    using Fitz.Metrics.Extensions;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Fitz.Database;
    using Microsoft.EntityFrameworkCore;
    using Pomelo.EntityFrameworkCore.MySql;

    /// <summary>
    /// This is the main class which bloon requires in order to go live. Its the brains of the entire setup.
    /// </summary>
#pragma warning disable CA1001 // Types that own disposable fields should be disposable

    public class Program
    {
        private static readonly SystemConsoleTheme BloonConsoleTheme = new SystemConsoleTheme(
            new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
            {
                [ConsoleThemeStyle.Text] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.SecondaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.TertiaryText] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.Invalid] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.Null] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Name] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.String] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Cyan },
                [ConsoleThemeStyle.Number] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Magenta },
                [ConsoleThemeStyle.Boolean] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Blue },
                [ConsoleThemeStyle.Scalar] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Green },
                [ConsoleThemeStyle.LevelVerbose] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.DarkGray },
                [ConsoleThemeStyle.LevelDebug] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Gray },
                [ConsoleThemeStyle.LevelInformation] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White },
                [ConsoleThemeStyle.LevelWarning] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.Yellow },
                [ConsoleThemeStyle.LevelError] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
                [ConsoleThemeStyle.LevelFatal] = new SystemConsoleThemeStyle { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
            });

        public static int Main()
        {
            // DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { "/config/.env" }));
            DotEnv.Load();

            // Configure logging
            // Log everything to console
            // Only log warning level and above to file
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
#if DEBUG
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: LogEventLevel.Debug, theme: BloonConsoleTheme)
#endif
                .WriteTo.File(
                    "Logs/.log",
                    outputTemplate: "[{Timestamp:HH:mm:ss}][{Level:u3}]{Message:lj}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                var host = CreateHostBuilder().Build();

                var connectionString = DatabaseConnection.ConnectionString;
                ServerVersion? serverVersion = null;
                try
                {
                    serverVersion = ServerVersion.AutoDetect(connectionString);
                }
                catch
                {
                    serverVersion = null;
                }

                if (serverVersion != null)
                {
                    try
                    {
                        using var scope = host.Services.CreateScope();
                        using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                        Log.Information("Applying database migrations...");
                        db.Database.Migrate();
                        Log.Information("Database migrations applied successfully");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to apply database migrations");
                        throw;
                    }
                }

                host.Run();
                return 0;
            }
#pragma warning disable CA1031 // Catch all exceptions
            catch (Exception ex)
#pragma warning restore CA1031
            {
                Log.Fatal(ex, "Bot terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder()
        {
            var metricsPort = Environment.GetEnvironmentVariable("METRICS_PORT") ?? "9090";
            var metricsUrl = $"http://localhost:{metricsPort}";

            return Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(metricsUrl);
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseOpenTelemetryPrometheusScrapingEndpoint();
                    });
                })
                .UseSerilog();
        }

        private static void ConfigureServices(HostBuilderContext ctx, IServiceCollection services)
        {
            // Register feature services
            foreach (Type type in Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => typeof(IServiceRegistrant).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract))
            {
                IServiceRegistrant registrant = Activator.CreateInstance(type) as IServiceRegistrant;
                registrant.ConfigureServices(services);
            }
            
            services.AddFitzMetrics();
            
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: "fitz-bot", serviceVersion: "1.0.0"))
                .WithMetrics(metrics =>
                {
                    metrics.AddMeter("Fitz.Metrics");
                    metrics.AddPrometheusExporter();
                });
            
            services.AddHostedService<Service>();
            Log.Information("Services configured");
        }
    }
}