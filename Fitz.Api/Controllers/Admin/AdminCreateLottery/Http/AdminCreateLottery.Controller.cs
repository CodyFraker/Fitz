using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Admin.AdminCreateLottery.Domain;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin.AdminCreateLottery.Http;

[ApiController]
[Route("api/admin/lottery")]
public class AdminCreateLotteryController(AdminCreateLotteryFacade adminCreateLotteryFacade, ILogger<AdminCreateLotteryController> logger, FitzMetrics? fitzMetrics = null) : ControllerBase
{
    private readonly AdminCreateLotteryFacade _adminCreateLotteryFacade = adminCreateLotteryFacade;
    private readonly ILogger<AdminCreateLotteryController> _logger = logger;
    private readonly FitzMetrics? _fitzMetrics = fitzMetrics;

    [HttpPost]
    [RequireDiscordAuth]
    [RequireAdmin]
    public async Task<IActionResult> CreateLottery([FromBody] AdminCreateLotteryRequestDto request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var endpoint = "/api/admin/lottery";

        _fitzMetrics?.RecordApiRequest(endpoint, "POST");

        try
        {
            if (!ModelState.IsValid)
            {
                _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            _logger.LogInformation("Admin create lottery request received. StartDate: {StartDate}, EndDate: {EndDate}, Pool: {Pool}", 
                request.StartDate, request.EndDate, request.Pool);

            var command = request.ToCommand();

            var response = await _adminCreateLotteryFacade.Execute(command, cancellationToken);

            _logger.LogInformation("Admin create lottery completed successfully. Message: {Message}", response.Message);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = response.Message
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("Admin create lottery failed - invalid argument. Error: {Error}", ex.Message);
            _fitzMetrics?.RecordApiError(endpoint, "validation_error");

            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin create lottery failed - unexpected error");
            _fitzMetrics?.RecordApiError(endpoint, "internal_server_error");

            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        finally
        {
            stopwatch.Stop();
            _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
        }
    }
}
