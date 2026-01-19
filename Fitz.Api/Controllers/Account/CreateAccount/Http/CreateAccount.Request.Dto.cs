using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Account.CreateAccount.Http
{
    [DisplayName("CreateAccountRequest")]
    public record CreateAccountRequestDto
    {
        [Required]
        public required ulong AccountId { get; set; }

        [Required]
        public required string Username { get; set; }

        public ulong GuildId { get; set; } = 0;

        internal CreateAccountCommand ToCommand()
        {
            var now = DateTime.UtcNow;
            return new CreateAccountCommand(
                Id: AccountId,
                Username: Username,
                GuildId: GuildId,
                CreatedOn: now,
                LastSeenDate: now,
                SubscribedToLottery: false,
                SubscribeTickets: 1,
                Deactivated: false
            );
        }
    }
}
