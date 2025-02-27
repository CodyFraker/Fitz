using System;

namespace Fitz.Features.Accounts.Create.Domain
{
    public class CreateAccountService
    {
        public CreateAccountModel BuildAccount(CreateAccountCommand command)
        {
            var CreateAccountCommand = this.ValidateCreateAccountCommand(command);

            return new CreateAccountModel
            {
                Id = CreateAccountCommand.Id,
                Username = CreateAccountCommand.Username,
                Beer = 0,
                LifetimeBeer = 0,
                SafeBalance = 0,
                Favorability = 100,
                CreatedDate = CreateAccountCommand.CreatedDate,
                LastSeenDate = DateTime.Now,
                LastActivityDate = DateTime.Now,
                SubscribeToLottery = false,
                SubscribeTickets = 0,
                Deactivated = false,
            };
        }

        private CreateAccountCommand ValidateCreateAccountCommand(CreateAccountCommand command)
        {
            return command;
        }
    }
}