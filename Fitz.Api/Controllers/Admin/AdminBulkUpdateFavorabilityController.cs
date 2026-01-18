using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/favorability")]
    public class AdminBulkUpdateFavorabilityController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminBulkUpdateFavorabilityController(AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("users/bulk")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> BulkUpdateFavorability([FromBody] BulkUpdateFavorabilityRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/favorability/users/bulk";
            
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

                if (request.UserIds == null || request.UserIds.Length == 0)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "UserIds array cannot be empty"
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

                int successCount = 0;
                int failCount = 0;

                foreach (var userId in request.UserIds)
                {
                    var account = _accountService.FindAccount(userId);
                    if (account != null)
                    {
                        var result = await _accountService.SetFavorabilityAsync(account, request.Favorability);
                        if (result.Success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failCount++;
                        }
                    }
                    else
                    {
                        failCount++;
                    }
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Bulk update completed. Success: {successCount}, Failed: {failCount}"
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
        }
    }

    public class BulkUpdateFavorabilityRequest
    {
        public ulong[] UserIds { get; set; }
        public int Favorability { get; set; }
    }
}
