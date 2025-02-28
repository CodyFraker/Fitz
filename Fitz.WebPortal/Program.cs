using Fitz.WebPortal.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using MudBlazor;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using Fitz.Shared.Data;
using Fitz.Shared.Models;
using System.Security.Claims;
using Fitz.WebPortal.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, Fitz.WebPortal.Data.ServerAuthenticationStateProvider>();

// Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Add ScreenSizeService for responsive design
builder.Services.AddScoped<ScreenSizeService>();

// Add database context
builder.Services.AddDbContext<Fitz.Shared.Data.BotContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    options.UseMySql(connectionString, serverVersion);
});

// Register the WebPortal BotContext as a scoped service
builder.Services.AddScoped<Fitz.WebPortal.Data.BotContext>(provider => 
{
    var options = provider.GetRequiredService<DbContextOptions<Fitz.Shared.Data.BotContext>>();
    return new Fitz.WebPortal.Data.BotContext(options);
});

// Add authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Discord";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/accessdenied";
    options.Cookie.Name = "FitzAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    
    // Add event handlers for redirecting
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = context =>
        {
            context.Response.Redirect("/login");
            return Task.CompletedTask;
        },
        OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect("/accessdenied");
            return Task.CompletedTask;
        }
    };
})
.AddDiscord("Discord", options =>
{
    options.ClientId = builder.Configuration["Discord:ClientId"] ?? "1077680432147087400";
    options.ClientSecret = builder.Configuration["Discord:ClientSecret"] ?? "";
    
    // This must EXACTLY match what's registered in the Discord Developer Portal
    options.CallbackPath = "/signin-discord-callback"; 
    
    options.SaveTokens = true;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    
    // Add scopes for the permissions you need - match the scopes in the Discord OAuth URL
    options.Scope.Add("identify");
    options.Scope.Add("guilds");
    options.Scope.Add("email");
    options.Scope.Add("guilds.join");
    options.Scope.Add("guilds.members.read");
    
    // Map claims from Discord to ASP.NET Core Identity claims
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
    options.ClaimActions.MapJsonKey("avatar", "avatar");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    
    // Add detailed logging for authentication events
    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
    {
        OnCreatingTicket = context =>
        {
            Console.WriteLine($"Creating ticket for user: {context.Identity?.Name}");
            if (context.AccessToken != null && context.AccessToken.Length > 10)
            {
                Console.WriteLine($"Access token: {context.AccessToken.Substring(0, 10)}...");
            }
            return Task.CompletedTask;
        },
        
        OnTicketReceived = context =>
        {
            Console.WriteLine($"Ticket received for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        },
        
        OnRedirectToAuthorizationEndpoint = context =>
        {
            Console.WriteLine($"Redirecting to authorization endpoint: {context.RedirectUri}");
            
            // Extract the state parameter if it exists
            string state = context.Properties.Items.TryGetValue(".redirect", out var redirectUri) 
                ? redirectUri 
                : "/";
                
            // Append the state to the redirect URI if it's not already there
            if (!context.RedirectUri.Contains("state=") && !string.IsNullOrEmpty(state))
            {
                var uriBuilder = new UriBuilder(context.RedirectUri);
                var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);
                query["state"] = state;
                uriBuilder.Query = query.ToString();
                context.RedirectUri = uriBuilder.ToString();
            }
            
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        },
        
        OnRemoteFailure = context =>
        {
            Console.WriteLine($"Remote authentication failure: {context.Failure?.Message}");
            if (context.Failure?.StackTrace != null)
            {
                Console.WriteLine($"Failure stack trace: {context.Failure.StackTrace}");
            }
            
            // Provide more detailed error information
            var errorMessage = context.Failure?.Message ?? "Authentication failed";
            Console.WriteLine($"Detailed error: {errorMessage}");
            
            context.Response.Redirect($"/login?error={Uri.EscapeDataString(errorMessage)}");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireClaim("role", "Admin"));
});

var app = builder.Build();

// Ensure database is created with all tables
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<Fitz.Shared.Data.BotContext>();
        
        // First check if the database exists and create it if it doesn't
        bool created = context.Database.EnsureCreated();
        Console.WriteLine($"Database {(created ? "was created" : "already exists")}.");
        
        // Now let's verify that all required tables exist
        try
        {
            // Check if accounts table exists by querying it
            var accountCount = context.Accounts.Count();
            Console.WriteLine($"Accounts table exists with {accountCount} records.");
            
            // Check if transactions table exists by querying it
            var transactionCount = context.Transactions.Count();
            Console.WriteLine($"Transactions table exists with {transactionCount} records.");
            
            // Check if lotteries table exists by querying it
            var lotteryCount = context.Lotteries.Count();
            Console.WriteLine($"Lotteries table exists with {lotteryCount} records.");
            
            // Check if lottery entries table exists by querying it
            var entryCount = context.LotteryEntries.Count();
            Console.WriteLine($"Lottery entries table exists with {entryCount} records.");
        }
        catch (Exception tableEx)
        {
            Console.WriteLine($"Error verifying tables: {tableEx.Message}");
            Console.WriteLine("Attempting to run SQL script to create missing tables...");
            
            try
            {
                // Create tables manually using raw SQL
                context.Database.ExecuteSqlRaw(@"
                    -- Create accounts table if it doesn't exist
                    CREATE TABLE IF NOT EXISTS `accounts` (
                        `Id` bigint unsigned NOT NULL,
                        `Username` longtext CHARACTER SET utf8mb4 NULL,
                        `Beer` int NOT NULL DEFAULT 0,
                        `LifetimeBeer` int NOT NULL DEFAULT 0,
                        `SafeBalance` int NOT NULL DEFAULT 0,
                        `Favorability` int NOT NULL DEFAULT 0,
                        `CreatedDate` datetime(6) NOT NULL,
                        `LastSeenDate` datetime(6) NOT NULL,
                        `LastActivityDate` datetime(6) NOT NULL,
                        `SubscribeToLottery` tinyint(1) NOT NULL DEFAULT 0,
                        `SubscribeTickets` int NOT NULL DEFAULT 1,
                        `Deactivated` tinyint(1) NOT NULL DEFAULT 0,
                        CONSTRAINT `PK_accounts` PRIMARY KEY (`Id`)
                    ) CHARACTER SET=utf8mb4;

                    -- Create transactions table if it doesn't exist
                    CREATE TABLE IF NOT EXISTS `transactions` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `SenderId` bigint unsigned NOT NULL,
                        `RecipientId` bigint unsigned NOT NULL,
                        `Amount` int NOT NULL,
                        `Reason` longtext CHARACTER SET utf8mb4 NOT NULL,
                        `Timestamp` datetime(6) NOT NULL,
                        CONSTRAINT `PK_transactions` PRIMARY KEY (`Id`)
                    ) CHARACTER SET=utf8mb4;

                    -- Create lotteries table if it doesn't exist
                    CREATE TABLE IF NOT EXISTS `lotteries` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `PrizePool` int NOT NULL,
                        `StartDate` datetime(6) NOT NULL,
                        `DrawDate` datetime(6) NOT NULL,
                        `IsActive` tinyint(1) NOT NULL,
                        `WinnerId` bigint unsigned NULL,
                        CONSTRAINT `PK_lotteries` PRIMARY KEY (`Id`)
                    ) CHARACTER SET=utf8mb4;

                    -- Create lottery_entries table if it doesn't exist
                    CREATE TABLE IF NOT EXISTS `lottery_entries` (
                        `Id` int NOT NULL AUTO_INCREMENT,
                        `LotteryId` int NOT NULL,
                        `AccountId` bigint unsigned NOT NULL,
                        `EntryDate` datetime(6) NOT NULL,
                        CONSTRAINT `PK_lottery_entries` PRIMARY KEY (`Id`),
                        CONSTRAINT `FK_lottery_entries_lotteries_LotteryId` FOREIGN KEY (`LotteryId`) REFERENCES `lotteries` (`Id`) ON DELETE CASCADE
                    ) CHARACTER SET=utf8mb4;

                    -- Create EF migrations history table if it doesn't exist
                    CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
                        `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
                        `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
                        CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
                    ) CHARACTER SET=utf8mb4;

                    -- Insert the migration record to prevent future migration attempts
                    INSERT IGNORE INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
                    VALUES ('20250228010252_InitialCreate', '8.0.0');
                ");
                
                Console.WriteLine("Successfully created missing tables using SQL script.");
                
                // Seed initial lottery if needed
                if (!context.Lotteries.Any())
                {
                    context.Lotteries.Add(new Fitz.Shared.Models.Lottery
                    {
                        PrizePool = 100,
                        StartDate = DateTime.UtcNow,
                        DrawDate = DateTime.UtcNow.AddDays(7),
                        IsActive = true
                    });
                    context.SaveChanges();
                    Console.WriteLine("Created initial lottery.");
                }
            }
            catch (Exception sqlEx)
            {
                Console.WriteLine($"Error creating tables with SQL: {sqlEx.Message}");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while initializing the database: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Change the order of mapping to prioritize Blazor components
app.MapBlazorHub();
app.MapFallbackToPage("/_host");
// Keep RazorPages mapping for special pages like _Host.cshtml
app.MapRazorPages();

app.Run();
