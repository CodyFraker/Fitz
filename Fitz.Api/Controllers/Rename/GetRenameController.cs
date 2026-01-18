using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Database.Entities;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Rename
{
    [ApiController]
    [Route("api/rename")]
    public class GetRenameController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetRenameController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("{id}")]
        [RequireDiscordAuth]
        public IActionResult GetRename(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/rename/{id}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var rename = db.Renames.Find(id);
                if (rename == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<RenameResponse>
                    {
                        Success = false,
                        Message = "Rename not found"
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
