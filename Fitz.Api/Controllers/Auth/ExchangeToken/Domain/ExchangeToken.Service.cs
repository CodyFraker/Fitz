using Fitz.Api.Authentication;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fitz.Api.Controllers.Auth.ExchangeToken.Domain;

public class ExchangeTokenService(
    IOptionsMonitor<DiscordAuthenticationOptions> options,
    IHttpClientFactory httpClientFactory,
    ILogger<ExchangeTokenService> logger)
{
    private readonly IOptionsMonitor<DiscordAuthenticationOptions> _options = options;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<ExchangeTokenService> _logger = logger;

    public async Task<ExchangeTokenModel> ExecuteAsync(ExchangeTokenCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("ExchangeTokenService execution started. RedirectUri: {RedirectUri}, CodeLength: {CodeLength}", 
            command.RedirectUri, command.Code?.Length ?? 0);

        var discordOptions = _options.Get("Discord");

        if (string.IsNullOrEmpty(discordOptions.ClientId) || string.IsNullOrEmpty(discordOptions.ClientSecret))
        {
            _logger.LogError("OAuth configuration is missing. ClientId is set: {HasClientId}, ClientSecret is set: {HasClientSecret}",
                !string.IsNullOrEmpty(discordOptions.ClientId),
                !string.IsNullOrEmpty(discordOptions.ClientSecret));
            throw new InvalidOperationException("OAuth configuration is missing");
        }

        if (string.IsNullOrEmpty(command.RedirectUri))
        {
            _logger.LogError("Redirect URI is missing from request");
            throw new ArgumentException("Redirect URI is required.", nameof(command.RedirectUri));
        }

        if (!string.IsNullOrEmpty(discordOptions.RedirectUri) && 
            !string.Equals(discordOptions.RedirectUri, command.RedirectUri, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Redirect URI mismatch. Configured: {ConfiguredUri}, Requested: {RequestedUri}",
                discordOptions.RedirectUri,
                command.RedirectUri);
        }

        var httpClient = _httpClientFactory.CreateClient();
        
        var tokenRequest = new Dictionary<string, string>
        {
            { "client_id", discordOptions.ClientId ?? string.Empty },
            { "client_secret", discordOptions.ClientSecret ?? string.Empty },
            { "grant_type", "authorization_code" },
            { "code", command.Code ?? string.Empty },
            { "redirect_uri", command.RedirectUri ?? string.Empty }
        };

        var content = new FormUrlEncodedContent(tokenRequest);
        var response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            string errorMessage = "Failed to exchange authorization code for token";
            string? discordError = null;
            string? discordErrorDescription = null;
            
            try
            {
                var errorData = JsonSerializer.Deserialize<DiscordErrorResponse>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (errorData != null)
                {
                    discordError = errorData.Error;
                    discordErrorDescription = errorData.ErrorDescription;
                    
                    if (!string.IsNullOrEmpty(discordErrorDescription))
                    {
                        errorMessage = $"Discord OAuth error: {discordErrorDescription}";
                    }
                    else if (!string.IsNullOrEmpty(discordError))
                    {
                        errorMessage = $"Discord OAuth error: {discordError}";
                    }
                }
            }
            catch
            {
            }
            
            _logger.LogError("Discord OAuth token exchange failed. StatusCode: {StatusCode}, Error: {Error}, ErrorDescription: {ErrorDescription}",
                (int)response.StatusCode,
                discordError ?? "unknown",
                discordErrorDescription ?? "unknown");
            
            throw new InvalidOperationException(errorMessage);
        }

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        
        DiscordTokenResponse? tokenData = null;
        try
        {
            tokenData = JsonSerializer.Deserialize<DiscordTokenResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize Discord token response. ResponseContent: {ResponseContent}",
                responseContent);
            throw new InvalidOperationException("Invalid token response format from Discord");
        }

        if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
        {
            _logger.LogError("Invalid token response from Discord. ResponseContent: {ResponseContent}",
                responseContent);
            throw new InvalidOperationException("Invalid token response from Discord");
        }

        var model = ExchangeTokenModel.From(
            tokenData.AccessToken,
            tokenData.TokenType ?? "Bearer",
            tokenData.ExpiresIn,
            tokenData.RefreshToken,
            tokenData.Scope ?? string.Empty
        );

        _logger.LogInformation("ExchangeTokenModel created successfully");

        return model;
    }

    private class DiscordTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        
        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }
        
        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    private class DiscordErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
        
        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }
}
