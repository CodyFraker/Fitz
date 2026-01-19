using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Variables;
using Microsoft.Extensions.Logging;

namespace Fitz.Seeds.Seeds
{
    public static class SeedFitzAccount
    {
        public static async Task ExecuteAsync(BotContext context, ILogger logger)
        {
            try
            {
                var existingAccount = await context.Accounts.FindAsync(Users.Fitz);
                if (existingAccount != null)
                {
                    logger.LogInformation("Fitz account already exists, skipping seed");
                    return;
                }

                var account = new AccountEntity
                {
                    Id = Users.Fitz,
                    Username = "Fitz",
                    LifetimeBeer = 128,
                    Beer = 128,
                    Favorability = 100,
                    CreatedDate = DateTime.Now,
                    LastSeenDate = DateTime.Now,
                    LastActivityDate = DateTime.Now,
                    subscribeToLottery = false,
                    SubscribeTickets = 1,
                    safeBalance = 128,
                    Deactivated = false
                };

                context.Accounts.Add(account);
                await context.SaveChangesAsync();

                logger.LogInformation("Fitz account seeded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to seed Fitz account");
            }
        }
    }
}
