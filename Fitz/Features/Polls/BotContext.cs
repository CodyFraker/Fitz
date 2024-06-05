using Fitz.Features.Polls.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<PollOptions> PollsOptions { get; set; }
    }
}