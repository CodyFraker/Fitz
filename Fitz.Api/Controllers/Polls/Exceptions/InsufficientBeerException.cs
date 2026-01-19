namespace Fitz.Api.Controllers.Polls.Exceptions;

public class InsufficientBeerException(int requiredAmount, int currentBalance) 
    : Exception($"User does not have enough beer. Required: {requiredAmount}, Current: {currentBalance}")
{
    public readonly int RequiredAmount = requiredAmount;
    public readonly int CurrentBalance = currentBalance;
}
