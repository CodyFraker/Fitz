using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Rename;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename
{
    [ApiController]
    [Route("api/rename")]
    public class UpdateRenameStatusController : ControllerBase
    {
        private readonly RenameService _renameService;
        private readonly FitzMetrics? _fitzMetrics;

        public UpdateRenameStatusController(RenameService renameService, FitzMetrics? fitzMetrics = null)
        {
            _renameService = renameService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPatch("{id}/status")]
        [RequireDiscordAuth]
        public async Task<IActionResult> UpdateRenameStatus(int id, [FromBody] UpdateRenameStatusRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/rename/{id}/status";
            
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

                var result = await _renameService.SetRenameStatus(id, request.Status);

                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                if (result.Data == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<RenameResponse>
                    {
                        Success = false,
                        Message = "Rename not found"
                    });
                }

                var rename = result.Data as Fitz.Database.Entities.Renames;
                if (rename == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid response data"
                    });
                }

                var response = new RenameResponse
                {
                    Id = rename.Id,
                    OldName = rename.OldName,
                    NewName = rename.NewName,
                    AffectedUserId = rename.AffectedUserId,
                    RequestedUserId = rename.RequestedUserId,
                    Days = rename.Days,
                    Cost = rename.Cost,
                    Notified = rename.Notified,
                    Status = rename.Status,
                    StartDate = rename.StartDate,
                    Expiration = rename.Expiration,
                    Timestamp = rename.Timestamp
                };

                return Ok(new ApiResponse<RenameResponse>
                {
                    Success = true,
                    Message = result.Message,
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
