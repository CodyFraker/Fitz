using Fitz.Core.Models;
using Fitz.Features.Lottery.Create.Persistance;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Lottery.Create.Domain
{
    public class CreateLotteryConductor
    {
        private readonly CreateLotteryService _createLotteryService;
        private readonly CreateLotteryRepository _createLotteryRepository;

        public CreateLotteryConductor(
            CreateLotteryService createLotteryService,
            CreateLotteryRepository createLotteryRepository)
        {
            _createLotteryService = createLotteryService ?? throw new ArgumentNullException(nameof(createLotteryService));
            _createLotteryRepository = createLotteryRepository ?? throw new ArgumentNullException(nameof(createLotteryRepository));
        }

        public async Task<Result> CreateLottery(CreateLotteryCommand command)
        {
            try
            {
                // Build the lottery entity
                var lottery = _createLotteryService.BuildLottery(command);

                // Persist the lottery
                var success = await _createLotteryRepository.PersistLottery(lottery);

                if (success)
                {
                    return new Result(true, "Lottery created successfully", lottery);
                }
                else
                {
                    return new Result(false, "Failed to create lottery", null);
                }
            }
            catch (Exception ex)
            {
                return new Result(false, $"Error creating lottery: {ex.Message}", null);
            }
        }

        public async Task<Result> GetCurrentLottery()
        {
            try
            {
                var lottery = await _createLotteryRepository.GetCurrentLottery();

                if (lottery != null)
                {
                    return new Result(true, "Current lottery retrieved successfully", lottery);
                }
                else
                {
                    return new Result(false, "No active lottery found", null);
                }
            }
            catch (Exception ex)
            {
                return new Result(false, $"Error retrieving current lottery: {ex.Message}", null);
            }
        }
    }
}