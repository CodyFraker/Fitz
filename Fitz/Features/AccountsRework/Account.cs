using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fitz.Features.AccountsRework
{
    public class Account
    {
        /// <summary>
        /// Discord User ID
        /// </summary>
        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("username")]
        public string? Username { get; set; }

        /// <summary>
        /// The currency of the user.
        /// </summary>
        [Column("beer")]
        public int Beer { get; set; }

        /// <summary>
        /// Lifetime currency of the user.
        /// </summary>
        [Column("lifetime_beer")]
        public int LifetimeBeer { get; set; }

        /// <summary>
        /// The amount of beer a user wants before they stop auto-entering the lottery.
        /// </summary>
        [Column("safe_balance")]
        public int safeBalance { get; set; }

        /// <summary>
        /// How much the bot likes the user.
        /// </summary>
        public int Favorability { get; set; }

        /// <summary>
        /// When the user signed up for an account
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the user was seen online
        /// </summary>
        public DateTime LastSeenDate { get; set; }

        /// <summary>
        /// THe last time the user was active in the discord guild
        /// </summary>
        public DateTime LastActivityDate { get; set; }

        /// <summary>
        /// If the user wants to re-enter the lottery after every drawing.
        /// </summary>
        public bool subscribeToLottery { get; set; }

        /// <summary>
        /// Number of tickets a person wishes to buy every lottery.
        /// </summary>
        public int SubscribeTickets { get; set; }

        public bool Deactivated { get; set; }
    }
}