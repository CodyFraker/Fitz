namespace Fitz.Api.Controllers.Rename.CalculateRenameCost.Domain;

public record CalculateRenameCostModel(
    int Cost)
{
    public static CalculateRenameCostModel From(int cost)
    {
        return new CalculateRenameCostModel(
            Cost: cost
        );
    }
}
