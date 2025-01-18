using Fitz.Features.AccountsRework.Create.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<CreateAccountModel> Accounts { get; set; }
    }
}