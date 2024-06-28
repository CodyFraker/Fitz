using Fitz.Features.Blackjack.Modals;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<BlackjackGame> BlackjackGame { get; set; }
        public DbSet<BlackjackPlayers> BlackjackPlayers { get; set; }
    }
}