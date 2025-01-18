using Fitz.Core.Contexts;
using Fitz.Core.Discord;
using Fitz.Features.AccountsRework.Create.Discord;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.AccountsRework.Create.Domain
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

                db.Accounts.Add(createAccountModel);
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