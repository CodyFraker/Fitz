using Fitz.Database;
using Fitz.Seeds.Seeds;
using Microsoft.Extensions.Logging;

namespace Fitz.Seeds
{
    public static class SeedRunner
    {
        public static async Task RunSeedsAsync(BotContext context, ILogger logger)
        {
            logger.LogInformation("Starting database seed execution...");

            try
            {
                await SeedFitzAccount.ExecuteAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred during seed execution");
            }

            logger.LogInformation("Database seed execution completed");
        }
    }
}
