using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql;

namespace Fitz.Database
{
    public class BotContextFactory : IDesignTimeDbContextFactory<BotContext>
    {
        public BotContext CreateDbContext(string[] args)
        {
            ServerVersion? version = null;
            try
            {
                version = ServerVersion.AutoDetect(DatabaseConnection.ConnectionString);
            }
            catch
            {
                version = ServerVersion.Parse("8.0.21-mysql");
            }

            var optionsBuilder = new DbContextOptionsBuilder<BotContext>();
            optionsBuilder.UseMySql(DatabaseConnection.ConnectionString, version);

            return new BotContext(optionsBuilder.Options);
        }
    }
}
