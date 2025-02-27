using System;

namespace Fitz.Features.Lottery.Create.Domain
{
    public class CreateLotteryService
    {
        public Models.Lottery BuildLottery(CreateLotteryCommand command)
        {
            var validatedCommand = ValidateCreateLotteryCommand(command);

            return new Models.Lottery
            {
                StartDate = validatedCommand.StartDate,
                EndDate = validatedCommand.EndDate,
                Pool = validatedCommand.InitialPool,
                CurrentLottery = true,
                WinningTicket = null
            };
        }

        private CreateLotteryCommand ValidateCreateLotteryCommand(CreateLotteryCommand command)
        {
            // Additional validation could be added here if needed
            return command;
        }
    }
}