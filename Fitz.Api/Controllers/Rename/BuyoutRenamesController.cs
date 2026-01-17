using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Rename;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename
{
    [ApiController]
    [Route("api/rename")]
    public class BuyoutRenamesController : ControllerBase
    {
        private readonly RenameService _renameService;
        private readonly FitzMetrics? _fitzMetrics;

        public BuyoutRenamesController(RenameService renameService, FitzMetrics? fitzMetrics = null)
        {
            _renameService = renameService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("user/{userId}/buyout")]
        [RequireDiscordAuth]
        public async Task<IActionResult> BuyoutRenames(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/rename/user/{userId}/buyout";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "POST");
            
            try
            {
                var result = await _renameService.BuyoutRenameRequests(userId);

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
                    Message = result.Message
                });
            }
            finally
            {
                stopwatch.Stop();
                _fitzMetrics?.RecordApiRequestDuration(endpoint, stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}
