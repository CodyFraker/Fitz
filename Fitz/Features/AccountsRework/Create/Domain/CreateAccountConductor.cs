using Fitz.Features.AccountsRework.Create.Discord;
using System.Threading.Tasks;

namespace Fitz.Features.AccountsRework.Create.Domain
{
    public class CreateAccountConductor(CreateAccountService createAccountService, CreateAccountRepository createAccountRepository)
    {
        private readonly CreateAccountService CreateAccountService = createAccountService;
        private readonly CreateAccountRepository createAccountRepository = createAccountRepository;

        public async Task<CreateAccountResponse> CreateAccount(CreateAccountCommand command)
        {
            var AccountModel = CreateAccountService.BuildAccount(command);

            var PersistedAccount = await createAccountRepository.PersistAccount(AccountModel);

            return PersistedAccount;
        }
    }
}