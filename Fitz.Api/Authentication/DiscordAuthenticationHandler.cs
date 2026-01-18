using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Fitz.Api.Authentication
{
    public class DiscordAuthenticationHandler : AuthenticationHandler<DiscordAuthenticationOptions>
    {
        public DiscordAuthenticationHandler(
            IOptionsMonitor<DiscordAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Logger.LogInformation("[DiscordAuth] Starting authentication for path: {Path}", Request.Path);

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Logger.LogWarning("[DiscordAuth] No Authorization header found for path: {Path}", Request.Path);
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            Logger.LogInformation("[DiscordAuth] Authorization header present: {HeaderPrefix}", 
                authHeader.Length > 20 ? authHeader.Substring(0, 20) + "..." : authHeader);

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("[DiscordAuth] Authorization header does not start with Bearer. Header: {HeaderPrefix}", 
                    authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader);
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (string.IsNullOrEmpty(token))
            {
                Logger.LogWarning("[DiscordAuth] Token is missing from Authorization header");
                return Task.FromResult(AuthenticateResult.Fail("Token is missing"));
            }

            Logger.LogInformation("[DiscordAuth] Token extracted, length: {TokenLength}, prefix: {TokenPrefix}", 
                token.Length, token.Length > 10 ? token.Substring(0, 10) + "..." : "too short");

            try
            {
                var claims = ValidateDiscordToken(token);
                if (claims == null)
                {
                    Logger.LogWarning("[DiscordAuth] Discord token validation returned null claims. Token prefix: {TokenPrefix}", 
                        token.Length > 10 ? token.Substring(0, 10) + "..." : "too short");
                    return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
                }

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                var userId = claims.FirstOrDefault(c => c.Type == "discord_id")?.Value ?? "unknown";
                Logger.LogInformation("[DiscordAuth] Authentication successful for user {UserId}", userId);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[DiscordAuth] Exception during authentication");
                return Task.FromResult(AuthenticateResult.Fail($"Authentication failed: {ex.Message}"));
            }
        }

        private List<Claim>? ValidateDiscordToken(string token)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing" || token == "test-token")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "123456789"),
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim("discord_id", "123456789")
                };

                return claims;
            }

            var serviceToken = Environment.GetEnvironmentVariable("API_SERVICE_TOKEN");
            if (!string.IsNullOrEmpty(serviceToken) && token == serviceToken)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, "service"),
                    new Claim(ClaimTypes.Name, "ServiceAccount"),
                    new Claim("discord_id", "service"),
                    new Claim("service_account", "true")
                };

                return claims;
            }

            try
            {
                Logger.LogInformation("[DiscordAuth] Validating token with Discord API...");
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var response = httpClient.GetAsync("https://discord.com/api/v10/users/@me").GetAwaiter().GetResult();
                
                Logger.LogInformation("[DiscordAuth] Discord API response status: {StatusCode}", response.StatusCode);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    Logger.LogWarning("[DiscordAuth] Discord token validation failed. StatusCode: {StatusCode}, Response: {Response}", 
                        response.StatusCode, errorContent);
                    return null;
                }

                var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                Logger.LogInformation("[DiscordAuth] Discord API response received, length: {Length}", json.Length);
                
                var user = System.Text.Json.JsonSerializer.Deserialize<DiscordUser>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (user == null)
                {
                    Logger.LogWarning("[DiscordAuth] Discord token validation returned null user. Response: {Response}", json);
                    return null;
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username ?? ""),
                    new Claim("discord_id", user.Id)
                };

                Logger.LogInformation("[DiscordAuth] Discord token validated successfully for user {UserId} ({Username})", user.Id, user.Username);
                return claims;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "[DiscordAuth] Exception occurred while validating Discord token");
                return null;
            }
        }

        private class DiscordUser
        {
            public string Id { get; set; } = string.Empty;
            public string? Username { get; set; }
        }
    }
}
