namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public record CalculateRenameCostResponse(
    int Cost)
{
    public static CalculateRenameCostResponse From(CalculateRenameCostModel model)
    {
        return new CalculateRenameCostResponse(
            Cost: model.Cost
        );
    }
}
