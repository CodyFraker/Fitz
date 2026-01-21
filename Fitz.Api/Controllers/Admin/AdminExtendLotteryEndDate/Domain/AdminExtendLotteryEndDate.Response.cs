namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;

public record AdminExtendLotteryEndDateResponse(
    string Message)
{
    public static AdminExtendLotteryEndDateResponse From(AdminExtendLotteryEndDateModel model)
    {
        return new AdminExtendLotteryEndDateResponse(Message: model.Message);
    }
}
