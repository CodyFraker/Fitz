using Fitz.Api.Authentication;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fitz.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class ExchangeTokenController : ControllerBase
    {
        private readonly DiscordAuthenticationOptions _options;
        private readonly FitzMetrics? _fitzMetrics;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExchangeTokenController> _logger;

        public ExchangeTokenController(
            IOptionsMonitor<DiscordAuthenticationOptions> options,
            IHttpClientFactory httpClientFactory,
            ILogger<ExchangeTokenController> logger,
            FitzMetrics? fitzMetrics = null)
        {
            _options = options.Get("Discord");
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("exchange-token")]
        public async Task<IActionResult> ExchangeToken([FromBody] ExchangeTokenRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/auth/exchange-token";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "POST");
            
            try
            {
                if (!ModelState.IsValid)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    var errors = new List<string>();
                    foreach (var modelStateEntry in ModelState.Values)
                    {
                        foreach (var error in modelStateEntry.Errors)
                        {
                            errors.Add(error.ErrorMessage);
                        }
                    }
                    _logger.LogError("Validation error in token exchange request. ModelState errors: {ModelStateErrors}",
                        string.Join(", ", errors));
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid request"
                    });
                }

                if (string.IsNullOrEmpty(_options.ClientId) || string.IsNullOrEmpty(_options.ClientSecret))
                {
                    _fitzMetrics?.RecordApiError(endpoint, "configuration_error");
                    _logger.LogError("OAuth configuration is missing. ClientId is set: {HasClientId}, ClientSecret is set: {HasClientSecret}",
                        !string.IsNullOrEmpty(_options.ClientId),
                        !string.IsNullOrEmpty(_options.ClientSecret));
                    return StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "OAuth configuration is missing"
                    });
                }

                if (string.IsNullOrEmpty(request.RedirectUri))
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    _logger.LogError("Redirect URI is missing from request");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Redirect URI is required"
                    });
                }

                if (!string.IsNullOrEmpty(_options.RedirectUri) && 
                    !string.Equals(_options.RedirectUri, request.RedirectUri, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Redirect URI mismatch. Configured: {ConfiguredUri}, Requested: {RequestedUri}",
                        _options.RedirectUri,
                        request.RedirectUri);
                }

                _logger.LogInformation("Exchanging authorization code. RedirectUri: {RedirectUri}, CodeLength: {CodeLength}, ClientId: {ClientIdPrefix}",
                    request.RedirectUri,
                    request.Code?.Length ?? 0,
                    _options.ClientId?.Substring(0, Math.Min(10, _options.ClientId?.Length ?? 0)) ?? "missing");

                var httpClient = _httpClientFactory.CreateClient();
                
                var tokenRequest = new Dictionary<string, string>
                {
                    { "client_id", _options.ClientId },
                    { "client_secret", _options.ClientSecret },
                    { "grant_type", "authorization_code" },
                    { "code", request.Code },
                    { "redirect_uri", request.RedirectUri }
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await httpClient.PostAsync("https://discord.com/api/oauth2/token", content);

                if (!response.IsSuccessStatusCode)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "discord_oauth_error");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
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
                    
                    _logger.LogError("Discord OAuth token exchange failed. StatusCode: {StatusCode}, Error: {Error}, ErrorDescription: {ErrorDescription}, ResponseContent: {ResponseContent}, RedirectUri: {RedirectUri}, ClientId: {ClientIdPrefix}",
                        (int)response.StatusCode,
                        discordError ?? "unknown",
                        discordErrorDescription ?? "unknown",
                        errorContent,
                        request.RedirectUri,
                        _options.ClientId?.Substring(0, Math.Min(10, _options.ClientId?.Length ?? 0)) ?? "missing");
                    
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = errorMessage
                    });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                
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
                    _fitzMetrics?.RecordApiError(endpoint, "deserialization_error");
                    _logger.LogError(ex, "Failed to deserialize Discord token response. ResponseContent: {ResponseContent}",
                        responseContent);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token response format from Discord"
                    });
                }

                if (tokenData == null || string.IsNullOrEmpty(tokenData.AccessToken))
                {
                    _fitzMetrics?.RecordApiError(endpoint, "invalid_token_response");
                    _logger.LogError("Invalid token response from Discord. ResponseContent: {ResponseContent}, TokenDataNull: {TokenDataNull}, AccessTokenEmpty: {AccessTokenEmpty}",
                        responseContent,
                        tokenData == null,
                        tokenData != null && string.IsNullOrEmpty(tokenData.AccessToken));
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid token response from Discord"
                    });
                }

                var authResponse = new AuthTokenResponse
                {
                    AccessToken = tokenData.AccessToken,
                    TokenType = tokenData.TokenType ?? "Bearer",
                    ExpiresIn = tokenData.ExpiresIn,
                    RefreshToken = tokenData.RefreshToken,
                    Scope = tokenData.Scope ?? string.Empty
                };

                return Ok(new ApiResponse<AuthTokenResponse>
                {
                    Success = true,
                    Data = authResponse
                });
            }
            catch (Exception ex)
            {
                _fitzMetrics?.RecordApiError(endpoint, "exception");
                _logger.LogError(ex, "Exception occurred while exchanging token");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while exchanging the token"
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
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
}
