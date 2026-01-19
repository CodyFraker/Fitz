namespace Fitz.Api.Controllers.Polls.Exceptions;

public class MaxPendingPollsReachedException(int currentCount, int maxCount) 
    : Exception($"You have reached the maximum number of pending polls. Current: {currentCount}, Max: {maxCount}")
{
    public readonly int CurrentCount = currentCount;
    public readonly int MaxCount = maxCount;
}
