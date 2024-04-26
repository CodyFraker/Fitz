using Fitz.Features.Polls.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Poll> Polls { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<PollOptions> PollsOptions { get; set; }
    }
}