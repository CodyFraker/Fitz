using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/favorability")]
    public class AdminUpdateFavorabilityController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminUpdateFavorabilityController(AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("users/{userId}")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> UpdateFavorability(ulong userId, [FromBody] UpdateFavorabilityRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/favorability/users/{userId}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "PATCH");
            
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

                var account = _accountService.FindAccount(userId);
                if (account == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Account not found"
                    });
                }

                if (request.Favorability < 0 || request.Favorability > 100)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Favorability must be between 0 and 100"
                    });
                }

                var result = await _accountService.SetFavorabilityAsync(account, request.Favorability);
                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Favorability updated successfully"
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
        }
    }

    public class UpdateFavorabilityRequest
    {
        public int Favorability { get; set; }
    }
}
