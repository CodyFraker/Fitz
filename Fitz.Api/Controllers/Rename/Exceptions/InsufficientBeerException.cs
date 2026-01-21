namespace Fitz.Api.Controllers.Rename.Exceptions;

public class InsufficientBeerException(int requiredAmount, int currentBalance) 
    : Exception($"Insufficient beer. Required: {requiredAmount}, Available: {currentBalance}")
{
    public readonly int RequiredAmount = requiredAmount;
    public readonly int CurrentBalance = currentBalance;
}
