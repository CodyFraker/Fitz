using System;

namespace Fitz.Features.Lottery.Create.Discord
{
    public class CreateLotteryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int InitialPool { get; set; }
    }
}
