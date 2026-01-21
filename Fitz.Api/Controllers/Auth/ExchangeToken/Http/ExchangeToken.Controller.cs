using Fitz.Api.Controllers.Auth.ExchangeToken.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Auth.ExchangeToken.Http;

[ApiController]
[Route("api/auth")]
public class ExchangeTokenController(ExchangeTokenFacade exchangeTokenFacade, ILogger<ExchangeTokenController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly ExchangeTokenFacade _exchangeTokenFacade = exchangeTokenFacade;
    private readonly ILogger<ExchangeTokenController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost("exchange-token")]
    public async Task<IActionResult> ExchangeToken([FromBody] ExchangeTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/auth/exchange-token";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

        try
        {
            if (!ModelState.IsValid)
            {
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
                _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            _logger.LogInformation("Exchange token request received. RedirectUri: {RedirectUri}, CodeLength: {CodeLength}", 
                request.RedirectUri, request.Code?.Length ?? 0);

            var command = request.ToCommand();

            var response = await _exchangeTokenFacade.Execute(command, cancellationToken);

            var dto = ExchangeTokenResponseDto.From(response);
            var authTokenResponse = dto.ToAuthTokenResponse();

            _logger.LogInformation("Exchange token completed successfully");

            return Ok(new ApiResponse<AuthTokenResponse>
            {
                Success = true,
                Data = authTokenResponse
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Exchange token failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("Exchange token failed - invalid operation. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, ex.Message.Contains("OAuth configuration") ? "configuration_error" : 
                ex.Message.Contains("Discord OAuth") ? "discord_oauth_error" : "invalid_token_response");

            var statusCode = ex.Message.Contains("OAuth configuration") ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest;

            return StatusCode(statusCode, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exchange token failed - unexpected error");
            _fitzMetrics?.RecordApiError(endpoint, "exception");

            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
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
}
