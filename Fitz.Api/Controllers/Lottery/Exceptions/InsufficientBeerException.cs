namespace Fitz.Api.Controllers.Lottery.Exceptions;

public class InsufficientBeerException(int requiredAmount, int currentBalance) 
    : Exception($"User does not have enough beer to buy tickets. Required: {requiredAmount}, Current: {currentBalance}")
{
    public readonly int RequiredAmount = requiredAmount;
    public readonly int CurrentBalance = currentBalance;
}
