using Fitz.Core.Models;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Accounts.Update.Persistence;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Accounts.Update.Domain
{
    public class UpdateAccountService
    {
        private readonly UpdateAccountRepository _repository;

        public UpdateAccountService(UpdateAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> UpdateAccountAsync(UpdateAccountCommand command)
        {
            try
            {
                // Get the account
                Account account = await _repository.GetAccountAsync(command.Id);
                if (account == null)
                {
                    return new Result(false, "Account not found.", null);
                }

                // Update properties if they are provided
                if (command.Username != null)
                {
                    account.Username = command.Username;
                }

                if (command.SafeBalance.HasValue)
                {
                    account.safeBalance = command.SafeBalance.Value;
                }

                if (command.SubscribeToLottery.HasValue)
                {
                    account.subscribeToLottery = command.SubscribeToLottery.Value;
                }

                if (command.SubscribeTickets.HasValue)
                {
                    account.SubscribeTickets = command.SubscribeTickets.Value;
                }

                if (command.Favorability.HasValue)
                {
                    if (account.Favorability >= 100)
                    {
                        return new Result(false, "User already has max favorability.", account);
                    }
                    account.Favorability = command.Favorability.Value;
                }

                if (command.Deactivated.HasValue)
                {
                    account.Deactivated = command.Deactivated.Value;
                }

                if (command.LastSeenDate.HasValue)
                {
                    account.LastSeenDate = command.LastSeenDate.Value;
                }

                if (command.LastActivityDate.HasValue)
                {
                    account.LastActivityDate = command.LastActivityDate.Value;
                }

                // Save the changes
                await _repository.UpdateAccountAsync(account);
                
                Log.Debug($"Updated account for {account.Username} | {account.Id}");
                return new Result(true, "Account updated successfully.", account);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to update account {command.Id}");
                return new Result(false, $"Failed to update account: {ex.Message}", null);
            }
        }
    }
} 