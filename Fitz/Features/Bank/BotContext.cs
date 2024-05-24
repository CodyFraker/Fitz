using Fitz.Features.Bank.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
    }
}