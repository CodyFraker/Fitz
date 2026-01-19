namespace Fitz.Api.Controllers.Account.CreateAccount.Domain
{
    public class CreateAccountFacade(CreateAccountService createAccountService, ICreateAccount createAccountQuery)
    {
        private readonly CreateAccountService _createAccountService = createAccountService;
        private readonly ICreateAccount _createAccountQuery = createAccountQuery;

        public async Task<CreateAccountResponse> Execute(CreateAccountCommand command, CancellationToken cancellationToken)
        {
            var model = await _createAccountService.ExecuteAsync(command, cancellationToken);

            await _createAccountQuery.Save(model, cancellationToken);

            return CreateAccountResponse.From(model);
        }
    }
}
