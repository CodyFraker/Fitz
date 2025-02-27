using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Features.Accounts.Create.Discord;
using Fitz.Features.Accounts.Models;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Create.Domain
{
    public sealed class CreateAccountRepository(IServiceScopeFactory scopeFactory, BotLog botLog)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly BotLog botLog = botLog;

        public async Task<CreateAccountResponse> PersistAccount(CreateAccountModel createAccountModel)
        {
            if (CheckForDuplicateAccount(createAccountModel))
            {
                Log.Debug($"{createAccountModel.Id} tried to create an account but already has one.");
                return new CreateAccountResponse
                {
                    StatusCode = System.Net.HttpStatusCode.Conflict,
                    Message = "User already has an account.",
                    Account = null
                };
            }

            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                // Convert CreateAccountModel to AccountEntity
                AccountEntity accountEntity = new AccountEntity
                {
                    Id = createAccountModel.Id,
                    Username = createAccountModel.Username,
                    Beer = createAccountModel.Beer,
                    LifetimeBeer = createAccountModel.LifetimeBeer,
                    safeBalance = createAccountModel.SafeBalance,
                    Favorability = createAccountModel.Favorability,
                    CreatedDate = createAccountModel.CreatedDate,
                    LastSeenDate = createAccountModel.LastSeenDate,
                    LastActivityDate = createAccountModel.LastActivityDate,
                    subscribeToLottery = createAccountModel.SubscribeToLottery,
                    SubscribeTickets = createAccountModel.SubscribeTickets,
                    Deactivated = createAccountModel.Deactivated
                };

                db.Accounts.Add(accountEntity);
                await db.SaveChangesAsync();

                return new CreateAccountResponse
                {
                    StatusCode = System.Net.HttpStatusCode.Created,
                    Message = "Account created successfully.",
                    Account = null,
                };
            }
            catch (Exception PersistAccountException)
            {
                Log.Error(PersistAccountException, $"Failed to persist account: {createAccountModel.Id}");
                return new CreateAccountResponse
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"Failed to persist account. Exception Message: {PersistAccountException.Message}",
                    Account = null
                };
            }
        }

        private bool CheckForDuplicateAccount(CreateAccountModel createAccountModel)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();
            return db.Accounts.Any(x => x.Id == createAccountModel.Id);
        }
    }
}