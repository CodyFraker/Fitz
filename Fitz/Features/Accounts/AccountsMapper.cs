using Fitz.Features.Accounts.Create.Discord;
using Fitz.Features.Accounts.Create.Domain;

namespace Fitz.Features.Accounts
{
    public sealed class AccountsMapper
    {
        public CreateAccountCommand MapCommandFromDto(CreateAccountDto dto)
        {
            return new CreateAccountCommand
            {
                Id = dto.Context.User.Id,
                Username = dto.Context.User.Username,
                CreatedDate = dto.Context.Interaction.CreationTimestamp.DateTime,
            };
        }
    }
}