using Fitz.Variables;

namespace Fitz.Api.Services
{
    public class AdminService
    {
        private static readonly HashSet<ulong> AdminIds = new()
        {
            Users.ProductionBot,
            Users.DevelopmentBot,
            Users.DukeofSussex,
            Users.Spy,
            Users.Dodecuplet,
            Users.Fitz
        };

        public static bool IsAdmin(ulong discordId)
        {
            return AdminIds.Contains(discordId);
        }
    }
}
