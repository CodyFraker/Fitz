using DSharpPlus;
using Fitz.Core.Services.Jobs;
using Fitz.Core.Discord;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Commands;
using Fitz.Features.Accounts.Update.Domain;
using Fitz.Features.Accounts.Update.Persistence;
using Fitz.Variables.Emojis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Jobs
{
    public class AccountJob : ITimedJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DiscordClient _discordClient;
        private readonly BotLog _botLog;

        public AccountJob(IServiceScopeFactory scopeFactory, DiscordClient dClient, BotLog botLog)
        {
            _scopeFactory = scopeFactory;
            _discordClient = dClient;
            _botLog = botLog;
        }

        public string Name => "Account Activity Check";

        public ulong Emoji => ManageRoleEmojis.Warning;

        public int Interval => 5;

        public async Task Execute()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                
                var accounts = await db.Accounts.ToListAsync();
                foreach (var account in accounts)
                {
                    var member = await _discordClient.GetUserAsync(account.Id);
                    if (member != null)
                    {
                        account.LastSeenDate = DateTime.UtcNow;
                        db.Accounts.Update(account);
                    }
                }
                
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _botLog.Error($"Error checking account activity: {ex.Message}");
            }
        }
    }
}