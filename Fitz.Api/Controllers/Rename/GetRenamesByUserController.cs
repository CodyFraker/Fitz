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
    public class GetRenamesByUserController : ControllerBase
    {
        private readonly RenameService _renameService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetRenamesByUserController(RenameService renameService, FitzMetrics? fitzMetrics = null)
        {
            _renameService = renameService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("user/{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetRenamesByUser(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/rename/user/{userId}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var renames = _renameService.GetRenamesByAccountId(userId);
                var response = renames.Select(r => new RenameResponse
                {
                    Id = r.Id,
                    OldName = r.OldName,
                    NewName = r.NewName,
                    AffectedUserId = r.AffectedUserId,
                    RequestedUserId = r.RequestedUserId,
                    Days = r.Days,
                    Cost = r.Cost,
                    Notified = r.Notified,
                    Status = r.Status,
                    StartDate = r.StartDate,
                    Expiration = r.Expiration,
                    Timestamp = r.Timestamp
                }).ToList();

                return Ok(new ApiResponse<List<RenameResponse>>
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
