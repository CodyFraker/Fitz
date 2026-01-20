using Fitz.Api.Controllers.Rename.CalculateRenameCost.Http;

namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public record CalculateRenameCostCommand(
    ulong AffectedUserId,
    ulong RequestedUserId,
    double Days,
    string NewName)
{
    public static CalculateRenameCostCommand From(CalculateRenameCostRequestDto request)
    {
        return new CalculateRenameCostCommand(
            AffectedUserId: request.AffectedUserId,
            RequestedUserId: request.RequestedUserId,
            Days: request.Days,
            NewName: request.NewName
        );
    }
}
