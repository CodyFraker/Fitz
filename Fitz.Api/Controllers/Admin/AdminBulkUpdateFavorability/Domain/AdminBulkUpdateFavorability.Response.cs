namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;

public record AdminBulkUpdateFavorabilityResponse(
    int SuccessCount,
    int FailCount,
    string Message)
{
    public static AdminBulkUpdateFavorabilityResponse From(AdminBulkUpdateFavorabilityModel model)
    {
        return new AdminBulkUpdateFavorabilityResponse(SuccessCount: model.SuccessCount, FailCount: model.FailCount, Message: model.Message);
    }
}
