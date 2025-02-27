using System;

namespace Fitz.Features.Lottery.Create.Domain
{
    public class CreateLotteryCommand
    {
        /// <summary>
        /// Start date of the lottery drawing.
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        /// End date of the lottery drawing.
        /// </summary>
        public DateTime EndDate { get; }

        /// <summary>
        /// Initial prize pool amount.
        /// </summary>
        public int InitialPool { get; }

        public CreateLotteryCommand(DateTime startDate, DateTime endDate, int initialPool)
        {
            if (endDate <= startDate)
            {
                throw new ArgumentException("End date must be after start date", nameof(endDate));
            }

            if (initialPool < 0)
            {
                throw new ArgumentException("Initial pool cannot be negative", nameof(initialPool));
            }

            StartDate = startDate;
            EndDate = endDate;
            InitialPool = initialPool;
        }
    }
}
