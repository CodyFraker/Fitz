using Fitz.Features.AccountsRework.Create.Discord;

namespace Fitz.Features.AccountsRework
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