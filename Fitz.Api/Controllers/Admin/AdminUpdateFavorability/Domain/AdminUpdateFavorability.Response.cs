namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;

public record AdminUpdateFavorabilityResponse(
    string Message)
{
    public static AdminUpdateFavorabilityResponse From(AdminUpdateFavorabilityModel model)
    {
        return new AdminUpdateFavorabilityResponse(Message: model.Message);
    }
}
