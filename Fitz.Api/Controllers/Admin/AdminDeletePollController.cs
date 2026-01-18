using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/polls")]
    public class AdminDeletePollController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public AdminDeletePollController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpDelete("{id}")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public async Task<IActionResult> DeletePoll(int id)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/polls/{id}";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "DELETE");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var poll = await db.Polls.FirstOrDefaultAsync(p => p.Id == id);

                if (poll == null)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "not_found");
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Poll not found"
                    });
                }

                var votes = await db.Votes.Where(v => v.PollId == poll.Id).ToListAsync();
                var options = await db.PollsOptions.Where(o => o.PollId == poll.Id).ToListAsync();
                
                db.Votes.RemoveRange(votes);
                db.PollsOptions.RemoveRange(options);
                db.Polls.Remove(poll);
                await db.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Poll {id} deleted successfully"
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
