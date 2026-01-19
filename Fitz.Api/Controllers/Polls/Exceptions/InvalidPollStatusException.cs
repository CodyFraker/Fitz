namespace Fitz.Api.Controllers.Polls.Exceptions;

public class InvalidPollStatusException(string expectedStatus, string actualStatus) 
    : Exception($"Invalid poll status. Expected: {expectedStatus}, Actual: {actualStatus}")
{
    public readonly string ExpectedStatus = expectedStatus;
    public readonly string ActualStatus = actualStatus;
}
