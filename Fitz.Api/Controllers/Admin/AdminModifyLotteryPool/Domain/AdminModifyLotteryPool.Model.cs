namespace Fitz.Api.Controllers.Admin.AdminModifyLotteryPool.Domain;

public record AdminModifyLotteryPoolModel(
    string Message)
{
    public static AdminModifyLotteryPoolModel From(string message)
    {
        return new AdminModifyLotteryPoolModel(Message: message);
    }
}
