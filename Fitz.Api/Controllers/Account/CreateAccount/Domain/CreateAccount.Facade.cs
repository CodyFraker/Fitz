namespace Fitz.Api.Controllers.Account.CreateAccount.Domain
{
    public class CreateAccountFacade(CreateAccountService createAccountService, ICreateAccount createAccountQuery, ILogger<CreateAccountFacade> logger)
    {
        private readonly CreateAccountService _createAccountService = createAccountService;
        private readonly ICreateAccount _createAccountQuery = createAccountQuery;
        private readonly ILogger<CreateAccountFacade> _logger = logger;

        public async Task<CreateAccountResponse> Execute(CreateAccountCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("CreateAccountFacade execution started. AccountId: {AccountId}, Username: {Username}", command.Id, command.Username);
            
            var model = await _createAccountService.ExecuteAsync(command, cancellationToken);
            
            _logger.LogInformation("CreateAccountService execution completed. AccountId: {AccountId}", command.Id);

            await _createAccountQuery.Save(model, cancellationToken);
            
            _logger.LogInformation("Account saved to persistence. AccountId: {AccountId}", command.Id);

            var response = CreateAccountResponse.From(model);
            
            _logger.LogInformation("CreateAccountFacade execution completed successfully. AccountId: {AccountId}, Username: {Username}", command.Id, command.Username);
            
            return response;
        }
    }
}
