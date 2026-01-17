using dotenv.net;
using Fitz.Api.Authentication;
using Fitz.Core.Contexts;
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
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using System.Reflection;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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

var connectionString = BotContext.ConnectionString;
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
        options.UseMySql(connectionString, serverVersion));
}
else
{
    builder.Services.AddDbContext<BotContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:Origins"]?.Split(',') ?? new[] { "http://localhost:3000" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddAuthentication("Discord")
    .AddScheme<DiscordAuthenticationOptions, DiscordAuthenticationHandler>("Discord", options =>
    {
        options.ClientId = builder.Configuration["Discord:ClientId"] ?? string.Empty;
        options.ClientSecret = builder.Configuration["Discord:ClientSecret"] ?? string.Empty;
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
}
