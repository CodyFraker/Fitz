namespace Fitz.Api.Controllers.Lottery.Exceptions;

public class LotteryNotFound : Exception
{
    public LotteryNotFound() : base("No active lottery found")
    {
    }
}
