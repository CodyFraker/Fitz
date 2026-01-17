using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Accounts;
using Fitz.Features.Rename;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename
{
    [ApiController]
    [Route("api/rename")]
    public class CalculateRenameCostController : ControllerBase
    {
        private readonly RenameService _renameService;
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public CalculateRenameCostController(RenameService renameService, AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _renameService = renameService;
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("calculate-cost")]
        [RequireDiscordAuth]
        public IActionResult CalculateRenameCost([FromBody] CalculateRenameCostRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/rename/calculate-cost";
            
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

                var affectedUser = _accountService.FindAccount(request.AffectedUserId);
                if (affectedUser == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Affected user account not found"
                    });
                }

                var requestedUser = _accountService.FindAccount(request.RequestedUserId);
                if (requestedUser == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Requested user account not found"
                    });
                }

                var cost = _renameService.GenerateRenameCost(affectedUser, requestedUser, request.Days, request.NewName);

                var response = new RenameCostResponse
                {
                    Cost = cost
                };

                return Ok(new ApiResponse<RenameCostResponse>
                {
                    Success = true,
                    Data = response
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
