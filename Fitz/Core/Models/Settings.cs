using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Core.Models
{
    [Table("settings")]
    public sealed class Settings
    {
        [Column("id"), Key]
        public int Id { get; set; }

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

        [Column("happy_hour_base_amount")]
        public int BaseHappyHourAmount { get; set; }

        [Column("account_creation_bonus_amount")]
        public int AccountCreationBonusAmount { get; set; }

        [Column("rename_base_cost")]
        public int RenameBaseCost { get; set; }

        [Column("poll_approved_bonus")]
        public int PollApprovedBonus { get; set; }

        [Column("poll_submitted_penalty")]
        public int PollSubmittedPenalty { get; set; }

        [Column("poll_declined_penalty")]
        public int PollDeclinedPenalty { get; set; }

        [Column("poll_vote")]
        public int PollVote { get; set; }

        [Column("poll_creator_tip")]
        public int PollCreatorTip { get; set; }

        [Column("max_pending_polls")]
        public int MaxPendingPolls { get; set; }
    }
}