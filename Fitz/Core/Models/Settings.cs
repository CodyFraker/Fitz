using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Core.Models
{
    [Table("settings")]
    public sealed class Settings
    {
        [Column("lottery_duration")]
        public int LotteryDuration { get; set; }

        [Column("lottery_pool")]
        public int BaseLotteryPool { get; set; }

        [Column("pool_rollover")]
        public bool LotteryPoolRollover { get; set; }

        [Column("ticket_cost")]
        public int TicketCost { get; set; }

        [Column("max_tickets")]
        public int MaxTickets { get; set; }

        [Column("base_happy_hour_amount")]
        public int BaseHappyHourAmount { get; set; }
    }
}