namespace Fitz.Api.Controllers.Admin.AdminExtendLotteryEndDate.Domain;

public record AdminExtendLotteryEndDateModel(
    string Message)
{
    public static AdminExtendLotteryEndDateModel From(string message)
    {
        return new AdminExtendLotteryEndDateModel(Message: message);
    }
}
