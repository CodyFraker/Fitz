namespace Fitz.Models
{
    using Fitz.BackgroundServices.Models;
    using Fitz.DB.Models;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;

    public class FitzContext : DbContext
    {
        public FitzContext(DbContextOptions<FitzContext> options)
            : base(options)
        {
        }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Beers> Beer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.None,
            };

            base.OnModelCreating(modelBuilder);
        }
    }
}
