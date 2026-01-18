using Fitz.Api.Authentication;
using Fitz.Variables;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Fitz.Api.Tests
{
    public class TestAuthenticationHandler : AuthenticationHandler<DiscordAuthenticationOptions>
    {
        public TestAuthenticationHandler(
            IOptionsMonitor<DiscordAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var adminId = Users.Spy.ToString();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, adminId),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim("discord_id", adminId)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
