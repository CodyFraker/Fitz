namespace Fitz.Api.Controllers.Lottery.Exceptions;

public class MaxTicketsReachedException(int currentTicketCount, int maxTickets) 
    : Exception($"User already has max amount of tickets. Current: {currentTicketCount}, Max: {maxTickets}")
{
    public readonly int CurrentTicketCount = currentTicketCount;
    public readonly int MaxTickets = maxTickets;
}
