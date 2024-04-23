using Microsoft.EntityFrameworkCore;
using Bot.Features.FAQ.Models;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Faq> Faqs { get; set; }
    }
}
