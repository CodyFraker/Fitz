using Fitz.Features.Accounts.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Account> Accounts_Old { get; set; }
    }
}