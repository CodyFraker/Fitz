using Fitz.Api.Attributes;
using Fitz.Api.Extensions;
using Fitz.Api.Models.Responses;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class GetCurrentUserController : ControllerBase
    {
        private readonly FitzMetrics? _fitzMetrics;
        private readonly ILogger<GetCurrentUserController>? _logger;

        public GetCurrentUserController(FitzMetrics? fitzMetrics = null, ILogger<GetCurrentUserController>? logger = null)
        {
            _fitzMetrics = fitzMetrics;
            _logger = logger;
        }

        [HttpGet("me")]
        [RequireDiscordAuth]
        public IActionResult GetCurrentUser()
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/auth/me";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var userId = User.RequireDiscordUserId();
                var username = User.GetDiscordUsername();
                var isAdmin = User.IsAdmin();

                _logger?.LogInformation("[GetCurrentUser] UserId: {UserId}, Username: {Username}, IsAdmin: {IsAdmin}", userId, username, isAdmin);

                var response = new CurrentUserResponse
                {
                    Id = userId,
                    Username = username,
                    IsAdmin = isAdmin
                };

                return Ok(new ApiResponse<CurrentUserResponse>
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
