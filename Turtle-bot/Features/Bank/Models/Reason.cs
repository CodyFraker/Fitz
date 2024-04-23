using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Features.Bank.Models
{
    public enum Reason
    {
        /// <summary>
        /// Whenever someone creates an accounut they get beer
        /// </summary>
        AccountCreationBonus = 1,

        /// <summary>
        /// Awarded or given beer
        /// </summary>
        Bonus = 2,

        /// <summary>
        /// When someone gives beer to another.
        /// </summary>
        Donated = 3,

        /// <summary>
        /// Whenever someone buys a ticket for the lottery
        /// </summary>
        Lotto = 4,
    }
}