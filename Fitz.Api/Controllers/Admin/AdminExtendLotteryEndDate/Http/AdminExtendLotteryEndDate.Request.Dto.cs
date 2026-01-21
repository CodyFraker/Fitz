using Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Http;

[DisplayName("AdminExtendLotteryEndDateRequest")]
public record AdminExtendLotteryEndDateRequestDto
{
    [Required]
    public required DateTime EndDate { get; set; }

    internal AdminExtendLotteryEndDateCommand ToCommand()
    {
        return AdminExtendLotteryEndDateCommand.From(this);
    }
}
