using Fitz.Features.Lottery.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Lottery> Drawing { get; set; }
        public DbSet<Ticket> Ticket { get; set; }

        /// <summary>
        /// Table containing lottery winners and their payouts.
        /// </summary>
        public DbSet<Winners> Winners { get; set; }
    }
}