using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.CreateAccount.Domain
{
    public class CreateAccountService(ICreateAccount createAccount)
    {
        private readonly ICreateAccount _createAccount = createAccount;

        public async Task<CreateAccountModel> ExecuteAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
        {
            if (command.Id == 0)
            {
                throw new ArgumentException("Account ID cannot be 0.", nameof(command.Id));
            }

            var existingAccount = await _createAccount.FindByIdAsync(command.Id, cancellationToken);
            if (existingAccount != null)
            {
                throw new AccountAlreadyExists(command.Id, command.Username);
            }

            return CreateAccountModel.From(command);
        }
    }
}
