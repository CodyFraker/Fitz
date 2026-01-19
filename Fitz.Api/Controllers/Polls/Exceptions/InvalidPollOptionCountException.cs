namespace Fitz.Api.Controllers.Polls.Exceptions;

public class InvalidPollOptionCountException(string pollType, int minCount, int maxCount, int actualCount) 
    : Exception($"Invalid option count for {pollType} poll. Required: {minCount}-{maxCount}, Actual: {actualCount}")
{
    public readonly string PollType = pollType;
    public readonly int MinCount = minCount;
    public readonly int MaxCount = maxCount;
    public readonly int ActualCount = actualCount;
}
