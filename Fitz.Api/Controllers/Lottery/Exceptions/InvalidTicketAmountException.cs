namespace Fitz.Api.Controllers.Lottery.Exceptions;

public class InvalidTicketAmountException(int requestedAmount, string reason) 
    : Exception($"Invalid ticket amount: {requestedAmount}. {reason}")
{
    public readonly int RequestedAmount = requestedAmount;
    public readonly string Reason = reason;
}
