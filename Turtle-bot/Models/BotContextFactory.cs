namespace Fitz.Models
{
    using System;
    using Microsoft.EntityFrameworkCore;

    public class FitzContextFactory
    {
        public string ConnectionString
        {
            get
            {
                return $"Host={Environment.GetEnvironmentVariable("DB_HOST")};"
                + $"Port={Environment.GetEnvironmentVariable("DB_PORT")};"
                + $"Username={Environment.GetEnvironmentVariable("DB_USER")};"
                + $"Password={Environment.GetEnvironmentVariable("DB_PASS")};"
                + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
                + $"SSL Mode=none";
            }
        }

        public FitzContext Create()
        {
            DbContextOptionsBuilder<FitzContext> optionsBuilder = new DbContextOptionsBuilder<FitzContext>();
            optionsBuilder.UseMySql(this.ConnectionString);
            return new FitzContext(optionsBuilder.Options);
        }
    }
}
