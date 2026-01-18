using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Features.Accounts;
using Fitz.Features.Rename;
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
    public class CreateRenameController : ControllerBase
    {
        private readonly RenameService _renameService;
        private readonly AccountService _accountService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public CreateRenameController(RenameService renameService, AccountService accountService, IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _renameService = renameService;
            _accountService = accountService;
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost]
        [RequireDiscordAuth]
        public async Task<IActionResult> CreateRename([FromBody] CreateRenameRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/rename";
            
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

                if (requestedUser.Beer < cost)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "insufficient_funds");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Insufficient beer. Required: {cost}, Available: {requestedUser.Beer}"
                    });
                }

                var rename = new Renames
                {
                    NewName = request.NewName,
                    AffectedUserId = request.AffectedUserId,
                    RequestedUserId = request.RequestedUserId,
                    Days = request.Days,
                    Cost = cost,
                    Status = request.Status ?? RenameStatus.Pending,
                    StartDate = request.StartDate,
                    Expiration = request.Expiration,
                    Timestamp = DateTime.UtcNow
                };

                var result = await _renameService.RenameUserAsync(rename);

                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();
                var savedRename = await db.Renames
                    .Where(r => r.AffectedUserId == rename.AffectedUserId 
                        && r.RequestedUserId == rename.RequestedUserId 
                        && r.NewName == rename.NewName
                        && r.Timestamp >= rename.Timestamp.AddSeconds(-5))
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefaultAsync();

                if (savedRename == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<RenameResponse>
                    {
                        Success = false,
                        Message = "Rename created but could not be retrieved"
                    });
                }

                var response = new RenameResponse
                {
                    Id = savedRename.Id,
                    OldName = savedRename.OldName,
                    NewName = savedRename.NewName,
                    AffectedUserId = savedRename.AffectedUserId,
                    RequestedUserId = savedRename.RequestedUserId,
                    Days = savedRename.Days,
                    Cost = savedRename.Cost,
                    Notified = savedRename.Notified,
                    Status = savedRename.Status,
                    StartDate = savedRename.StartDate,
                    Expiration = savedRename.Expiration,
                    Timestamp = savedRename.Timestamp
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
