using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.CreateAccount.Domain
{
    public class CreateAccountService(ICreateAccount createAccount, ILogger<CreateAccountService> logger)
    {
        private readonly ICreateAccount _createAccount = createAccount;
        private readonly ILogger<CreateAccountService> _logger = logger;

        public async Task<CreateAccountModel> ExecuteAsync(CreateAccountCommand command, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("CreateAccountService execution started. AccountId: {AccountId}, Username: {Username}", command.Id, command.Username);
            
            if (command.Id == 0)
            {
                _logger.LogError("Account creation validation failed - Account ID cannot be 0. Username: {Username}", command.Username);
                throw new ArgumentException("Account ID cannot be 0.", nameof(command.Id));
            }

            var existingAccount = await _createAccount.FindByIdAsync(command.Id, cancellationToken);
            if (existingAccount != null)
            {
                _logger.LogWarning("Account creation failed - account already exists. AccountId: {AccountId}, Username: {Username}", command.Id, command.Username);
                throw new AccountAlreadyExists(command.Id, command.Username);
            }

            var model = CreateAccountModel.From(command);
            
            _logger.LogInformation("CreateAccountModel created successfully. AccountId: {AccountId}, Username: {Username}", command.Id, command.Username);
            
            return model;
        }
    }
}
