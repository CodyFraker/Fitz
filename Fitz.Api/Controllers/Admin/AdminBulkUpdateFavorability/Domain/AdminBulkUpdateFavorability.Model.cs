namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;

public record AdminBulkUpdateFavorabilityModel(
    int SuccessCount,
    int FailCount,
    string Message)
{
    public static AdminBulkUpdateFavorabilityModel From(int successCount, int failCount, string message)
    {
        return new AdminBulkUpdateFavorabilityModel(SuccessCount: successCount, FailCount: failCount, Message: message);
    }
}
