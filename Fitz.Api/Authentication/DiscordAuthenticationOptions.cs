using Microsoft.AspNetCore.Authentication;

namespace Fitz.Api.Authentication
{
    public class DiscordAuthenticationOptions : AuthenticationSchemeOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
    }
}
