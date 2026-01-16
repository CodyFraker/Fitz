using Microsoft.AspNetCore.Authorization;

namespace Fitz.Api.Attributes
{
    public class RequireDiscordAuthAttribute : AuthorizeAttribute
    {
        public RequireDiscordAuthAttribute()
        {
            AuthenticationSchemes = "Discord";
        }
    }
}
