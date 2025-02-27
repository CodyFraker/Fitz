using Fitz.Core.Models;

namespace Fitz.Features.Lottery.Create.Discord
{
    public class CreateLotteryResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Models.Lottery Lottery { get; set; }

        public static CreateLotteryResponse FromResult(Result result)
        {
            return new CreateLotteryResponse
            {
                Success = result.Success,
                Message = result.Message,
                Lottery = result.Data as Models.Lottery
            };
        }
    }
}
