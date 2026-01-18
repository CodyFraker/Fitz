using System.Security.Claims;
using Fitz.Api.Services;

namespace Fitz.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static ulong? GetDiscordUserId(this ClaimsPrincipal? principal)
        {
            if (principal == null)
            {
                return null;
            }

            var discordIdClaim = principal.FindFirst("discord_id")?.Value 
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(discordIdClaim))
            {
                return null;
            }

            if (ulong.TryParse(discordIdClaim, out var userId))
            {
                return userId;
            }

            return null;
        }

        public static string GetDiscordUsername(this ClaimsPrincipal? principal)
        {
            if (principal == null)
            {
                return string.Empty;
            }

            return principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static ulong RequireDiscordUserId(this ClaimsPrincipal? principal)
        {
            var userId = principal.GetDiscordUserId();
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User is not authenticated or Discord ID is missing");
            }

            return userId.Value;
        }

        public static bool IsAdmin(this ClaimsPrincipal? principal)
        {
            var userId = principal.GetDiscordUserId();
            if (userId == null)
            {
                return false;
            }

            return AdminService.IsAdmin(userId.Value);
        }
    }
}
