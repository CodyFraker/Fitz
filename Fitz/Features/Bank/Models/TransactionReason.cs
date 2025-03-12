namespace Fitz.Features.Bank.Models
{
    /// <summary>
    /// Represents the reason for a transaction
    /// </summary>
    public enum TransactionReason
    {
        /// <summary>
        /// Unknown reason
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// User donated to another user
        /// </summary>
        Donated = 1,

        /// <summary>
        /// Generic bonus
        /// </summary>
        Bonus = 2,

        /// <summary>
        /// Bonus for creating an account
        /// </summary>
        AccountCreationBonus = 3,

        /// <summary>
        /// Happy hour bonus
        /// </summary>
        HappyHour = 4,

        /// <summary>
        /// Admin added balance to user
        /// </summary>
        AdminAddBalance = 5,

        /// <summary>
        /// Admin removed balance from user
        /// </summary>
        AdminRemoveBalance = 6,

        /// <summary>
        /// User renamed themselves
        /// </summary>
        Rename = 7,

        /// <summary>
        /// User bought a lottery ticket
        /// </summary>
        Lotto = 8,

        /// <summary>
        /// User played a song
        /// </summary>
        GameBet = 9,

        /// <summary>
        /// User won the lottery
        /// </summary>
        GameWin = 10,

        /// <summary>
        /// User's poll was approved
        /// </summary>
        GameRefund = 11
    }
} 