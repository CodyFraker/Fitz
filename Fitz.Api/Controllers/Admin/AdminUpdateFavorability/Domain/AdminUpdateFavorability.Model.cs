namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;

public record AdminUpdateFavorabilityModel(
    string Message)
{
    public static AdminUpdateFavorabilityModel From(string message)
    {
        return new AdminUpdateFavorabilityModel(Message: message);
    }
}
