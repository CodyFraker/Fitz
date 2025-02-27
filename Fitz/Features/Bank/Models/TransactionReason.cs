namespace Fitz.Features.Bank.Models
{
    /// <summary>
    /// Represents the reason for a transaction
    /// </summary>
    public enum TransactionReason
    {
        /// <summary>
        /// Bonus for creating an account
        /// </summary>
        AccountCreationBonus,

        /// <summary>
        /// Generic bonus
        /// </summary>
        Bonus,

        /// <summary>
        /// User donated to another user
        /// </summary>
        Donated,

        /// <summary>
        /// User bought a lottery ticket
        /// </summary>
        Lotto,

        /// <summary>
        /// User won the lottery
        /// </summary>
        LottoWin,

        /// <summary>
        /// User renamed themselves
        /// </summary>
        Rename,

        /// <summary>
        /// User played a song
        /// </summary>
        MusicPlay,

        /// <summary>
        /// User skipped a song
        /// </summary>
        MusicSkip,

        /// <summary>
        /// Happy hour bonus
        /// </summary>
        HappyHour,

        /// <summary>
        /// User submitted a poll
        /// </summary>
        PollSubmitted,

        /// <summary>
        /// User's poll was approved
        /// </summary>
        PollApproved,

        /// <summary>
        /// User's poll was declined
        /// </summary>
        PollDeclined,

        /// <summary>
        /// User voted on a poll
        /// </summary>
        PollVote,

        /// <summary>
        /// User tipped a poll creator
        /// </summary>
        PollCreatorTip,
        
        /// <summary>
        /// Admin added balance to user
        /// </summary>
        AdminAddBalance,
        
        /// <summary>
        /// Admin removed balance from user
        /// </summary>
        AdminRemoveBalance
    }
} 