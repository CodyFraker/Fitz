using Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Http;

[DisplayName("AdminModifyLotteryPoolRequest")]
public record AdminModifyLotteryPoolRequestDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public required int Pool { get; set; }

    internal AdminModifyLotteryPoolCommand ToCommand()
    {
        return AdminModifyLotteryPoolCommand.From(this);
    }
}
