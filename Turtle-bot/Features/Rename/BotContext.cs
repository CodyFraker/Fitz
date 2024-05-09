using Fitz.Features.Rename.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Renames> Renames { get; set; }
    }
}