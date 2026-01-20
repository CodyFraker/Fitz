using dotenv.net;
using Fitz.Api;
using Fitz.Api.Authentication;
using Fitz.Database;
using Fitz.Core.Discord;
using Fitz.Core.Services;
using Fitz.Features.Accounts;
using Fitz.Features.Bank;
using Fitz.Features.Blackjack;
using Fitz.Features.HappyHour;
using Fitz.Features.Lottery;
using Fitz.Features.Polls;
using Fitz.Features.Rename;
using Fitz.Features.Settings;
using Fitz.Metrics;
using Fitz.Metrics.Extensions;
using Fitz.Seeds;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using System.Reflection;

try
{
    DotEnv.Load();
    //DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { "/config/.env" }));
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Failed to load .env file: {ex.Message}");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UlongToStringConverter());
    });
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fitz API", Version = "v1" });
    c.AddSecurityDefinition("Discord", new OpenApiSecurityScheme
    {
        Description = "Discord OAuth2 Bearer Token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Discord"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Discord"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
    builder.Services.AddDbContext<BotContext>(options =>
        options.UseMySql(connectionString, serverVersion),
        optionsLifetime: ServiceLifetime.Singleton);
    builder.Services.AddDbContextFactory<BotContext>(options =>
        options.UseMySql(connectionString, serverVersion));
}
else
{
    builder.Services.AddDbContext<BotContext>(options =>
        options.UseInMemoryDatabase("TestDb"),
        optionsLifetime: ServiceLifetime.Singleton);
    builder.Services.AddDbContextFactory<BotContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',') ?? new[] { "http://localhost:5173" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication("Discord")
    .AddScheme<DiscordAuthenticationOptions, DiscordAuthenticationHandler>("Discord", options =>
    {
        var clientId = builder.Configuration["DISCORD_CLIENT_ID"]
            ?? Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
        if (string.IsNullOrEmpty(clientId))
        {
            throw new Exception("Discord client ID is not set");
        }
        var clientSecret = builder.Configuration["DISCORD_CLIENT_SECRET"]
            ?? Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
        if (string.IsNullOrEmpty(clientSecret))
        {
            throw new Exception("Discord client secret is not set");
        }
        var redirectUri = builder.Configuration["DISCORD_REDIRECT_URI"]
            ?? Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI");
        if (string.IsNullOrEmpty(redirectUri))
        {
            throw new Exception("Discord redirect URI is not set");
        }

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.RedirectUri = redirectUri;
    });

builder.Services.AddAuthorization();

DSharpPlus.DiscordClient? mockDiscordClient = null;
try
{
    mockDiscordClient = new DSharpPlus.DiscordClient(new DSharpPlus.DiscordConfiguration
    {
        Token = "MOCK_TOKEN_FOR_API",
        TokenType = DSharpPlus.TokenType.Bot,
    });
    builder.Services.AddSingleton(mockDiscordClient);
}
catch { }

builder.Services.AddSingleton<BotLog>(sp =>
{
    var client = sp.GetService<DSharpPlus.DiscordClient>();
    return new BotLog(client ?? mockDiscordClient ?? throw new InvalidOperationException("DiscordClient required"));
});

builder.Services.AddSingleton<Fitz.Core.Discord.ActivityManager>(sp =>
{
    var client = sp.GetService<DSharpPlus.DiscordClient>();
    return new Fitz.Core.Discord.ActivityManager(client ?? mockDiscordClient ?? throw new InvalidOperationException("DiscordClient required"));
});

foreach (Type type in Assembly.Load("Fitz")
    .GetTypes()
    .Where(t => typeof(IServiceRegistrant).IsAssignableFrom(t)
        && !t.IsInterface
        && !t.IsAbstract))
{
    IServiceRegistrant? registrant = Activator.CreateInstance(type) as IServiceRegistrant;
    registrant?.ConfigureServices(builder.Services);
}

builder.Services.AddScoped<Fitz.Api.Controllers.Account.CreateAccount.Domain.ICreateAccount, Fitz.Api.Controllers.Account.CreateAccount.Persistence.CreateAccount>();
builder.Services.AddScoped<Fitz.Api.Controllers.Account.CreateAccount.Domain.CreateAccountService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Account.CreateAccount.Domain.CreateAccountFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Account.GetAccount.Domain.IGetAccount, Fitz.Api.Controllers.Account.GetAccount.Persistence.GetAccount>();
builder.Services.AddScoped<Fitz.Api.Controllers.Account.GetAccount.Domain.GetAccountService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Account.GetAccount.Domain.GetAccountFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain.IGetCurrentLottery, Fitz.Api.Controllers.Lottery.GetCurrentLottery.Persistence.GetCurrentLottery>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain.GetCurrentLotteryService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetCurrentLottery.Domain.GetCurrentLotteryFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.BuyTickets.Domain.IBuyTickets, Fitz.Api.Controllers.Lottery.BuyTickets.Persistence.BuyTickets>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.BuyTickets.Domain.BuyTicketsService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.BuyTickets.Domain.BuyTicketsFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain.IGetLotteryHistory, Fitz.Api.Controllers.Lottery.GetLotteryHistory.Persistence.GetLotteryHistory>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain.GetLotteryHistoryService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetLotteryHistory.Domain.GetLotteryHistoryFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain.IGetLotteryStatistics, Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Persistence.GetLotteryStatistics>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain.GetLotteryStatisticsService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Lottery.GetLotteryStatistics.Domain.GetLotteryStatisticsFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.CreatePoll.Domain.ICreatePoll, Fitz.Api.Controllers.Polls.CreatePoll.Persistence.CreatePoll>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.CreatePoll.Domain.CreatePollService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.CreatePoll.Domain.CreatePollFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPolls.Domain.IGetPolls, Fitz.Api.Controllers.Polls.GetPolls.Persistence.GetPolls>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPolls.Domain.GetPollsService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPolls.Domain.GetPollsFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPoll.Domain.IGetPoll, Fitz.Api.Controllers.Polls.GetPoll.Persistence.GetPoll>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPoll.Domain.GetPollService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPoll.Domain.GetPollFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollOptions.Domain.IGetPollOptions, Fitz.Api.Controllers.Polls.GetPollOptions.Persistence.GetPollOptions>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollOptions.Domain.GetPollOptionsService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollOptions.Domain.GetPollOptionsFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollVotes.Domain.IGetPollVotes, Fitz.Api.Controllers.Polls.GetPollVotes.Persistence.GetPollVotes>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollVotes.Domain.GetPollVotesService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollVotes.Domain.GetPollVotesFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetUserPolls.Domain.IGetUserPolls, Fitz.Api.Controllers.Polls.GetUserPolls.Persistence.GetUserPolls>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetUserPolls.Domain.GetUserPollsService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetUserPolls.Domain.GetUserPollsFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain.IGetPollsWithDetails, Fitz.Api.Controllers.Polls.GetPollsWithDetails.Persistence.GetPollsWithDetails>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain.GetPollsWithDetailsService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain.GetPollsWithDetailsFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.AddVote.Domain.IAddVote, Fitz.Api.Controllers.Polls.AddVote.Persistence.AddVote>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.AddVote.Domain.AddVoteService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.AddVote.Domain.AddVoteFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.UpdateVote.Domain.IUpdateVote, Fitz.Api.Controllers.Polls.UpdateVote.Persistence.UpdateVote>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.UpdateVote.Domain.UpdateVoteService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.UpdateVote.Domain.UpdateVoteFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.EvaluatePoll.Domain.IEvaluatePoll, Fitz.Api.Controllers.Polls.EvaluatePoll.Persistence.EvaluatePoll>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.EvaluatePoll.Domain.EvaluatePollService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.EvaluatePoll.Domain.EvaluatePollFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Polls.PostPollToPending.Domain.IPostPollToPending, Fitz.Api.Controllers.Polls.PostPollToPending.Persistence.PostPollToPending>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.PostPollToPending.Domain.PostPollToPendingService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Polls.PostPollToPending.Domain.PostPollToPendingFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.CreateRename.Domain.ICreateRename, Fitz.Api.Controllers.Rename.CreateRename.Persistence.CreateRename>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.CreateRename.Domain.CreateRenameService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.CreateRename.Domain.CreateRenameFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRename.Domain.IGetRename, Fitz.Api.Controllers.Rename.GetRename.Persistence.GetRename>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRename.Domain.GetRenameService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRename.Domain.GetRenameFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRenames.Domain.IGetRenames, Fitz.Api.Controllers.Rename.GetRenames.Persistence.GetRenames>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRenames.Domain.GetRenamesService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRenames.Domain.GetRenamesFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain.IGetRenamesByUser, Fitz.Api.Controllers.Rename.GetRenamesByUser.Persistence.GetRenamesByUser>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain.GetRenamesByUserService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.GetRenamesByUser.Domain.GetRenamesByUserFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain.IUpdateRenameStatus, Fitz.Api.Controllers.Rename.UpdateRenameStatus.Persistence.UpdateRenameStatus>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain.UpdateRenameStatusService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.UpdateRenameStatus.Domain.UpdateRenameStatusFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain.ICalculateRenameCost, Fitz.Api.Controllers.Rename.CalculateRenameCost.Persistence.CalculateRenameCost>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain.CalculateRenameCostService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain.CalculateRenameCostFacade>();

builder.Services.AddScoped<Fitz.Api.Controllers.Rename.BuyoutRenames.Domain.IBuyoutRenames, Fitz.Api.Controllers.Rename.BuyoutRenames.Persistence.BuyoutRenames>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.BuyoutRenames.Domain.BuyoutRenamesService>();
builder.Services.AddScoped<Fitz.Api.Controllers.Rename.BuyoutRenames.Domain.BuyoutRenamesFacade>();

builder.Services.AddFitzMetrics();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "fitz-api", serviceVersion: "1.0.0"))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddMeter("Fitz.Metrics");
        metrics.AddPrometheusExporter();
    });

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

if (serverVersion != null)
{
    try
    {
        using var scope = app.Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");

        logger.LogInformation("Running database seeds...");
        await SeedRunner.RunSeedsAsync(db, logger);
        logger.LogInformation("Database seeds completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to apply database migrations");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    var clientId = builder.Configuration["DISCORD_CLIENT_ID"] 
        ?? Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID");
    var clientSecret = builder.Configuration["DISCORD_CLIENT_SECRET"]
        ?? Environment.GetEnvironmentVariable("DISCORD_CLIENT_SECRET");
    var redirectUri = builder.Configuration["DISCORD_REDIRECT_URI"]
        ?? Environment.GetEnvironmentVariable("DISCORD_REDIRECT_URI");

    logger.LogInformation("Discord OAuth Configuration Loaded:");
    logger.LogInformation("  ClientId: {ClientIdStatus}", 
        string.IsNullOrEmpty(clientId) 
            ? "NOT SET" 
            : $"{clientId.Substring(0, Math.Min(10, clientId.Length))}...");
    logger.LogInformation("  ClientSecret: {ClientSecretStatus}", 
        string.IsNullOrEmpty(clientSecret) ? "NOT SET" : "***SET***");
    logger.LogInformation("  RedirectUri: {RedirectUri}", redirectUri);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();

namespace Fitz.Api
{
    public partial class Program { }

    public class UlongToStringConverter : System.Text.Json.Serialization.JsonConverter<ulong>
    {
        public override ulong Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType == System.Text.Json.JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (ulong.TryParse(stringValue, out var value))
                {
                    return value;
                }
            }
            else if (reader.TokenType == System.Text.Json.JsonTokenType.Number)
            {
                return reader.GetUInt64();
            }
            
            throw new System.Text.Json.JsonException($"Unable to convert {reader.TokenType} to ulong");
        }

        public override void Write(System.Text.Json.Utf8JsonWriter writer, ulong value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
