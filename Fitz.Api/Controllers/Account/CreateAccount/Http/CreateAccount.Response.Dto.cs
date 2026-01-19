using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.CreateAccount.Http
{
    [DisplayName("CreateAccountResponse")]
    public record CreateAccountResponseDto
    {
        [Required]
        public required ulong AccountId { get; set; }


        public static CreateAccountResponseDto From(CreateAccountResponse response)
        {
            return new CreateAccountResponseDto
            {
                AccountId = response.Id,
            };
        }
    }
}
