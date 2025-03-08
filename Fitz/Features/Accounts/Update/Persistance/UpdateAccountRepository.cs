using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Update.Persistence
{
    public class UpdateAccountRepository
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateAccountRepository(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<Account> GetAccountAsync(ulong id)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                Account entity = await db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
                
                if (entity == null)
                    return null;
                    
                // Convert AccountEntity to Account
                Account account = new Account
                {
                    Id = entity.Id,
                    Username = entity.Username,
                    Beer = entity.Beer,
                    LifetimeBeer = entity.LifetimeBeer,
                    safeBalance = entity.safeBalance,
                    Favorability = entity.Favorability,
                    CreatedDate = entity.CreatedDate,
                    LastSeenDate = entity.LastSeenDate,
                    LastActivityDate = entity.LastActivityDate,
                    subscribeToLottery = entity.subscribeToLottery,
                    SubscribeTickets = entity.SubscribeTickets,
                    Deactivated = entity.Deactivated
                };
                
                return account;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to get account {id}");
                return null;
            }
        }

        public async Task<bool> UpdateAccountAsync(Account account)
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
                
                db.Accounts.Update(account);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update account {account.Id}");
                return false;
            }
        }
    }
} 