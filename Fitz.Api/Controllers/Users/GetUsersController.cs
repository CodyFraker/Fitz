using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;

namespace Fitz.Api.Controllers.Users
{
    [ApiController]
    [Route("api/users")]
    public class GetUsersController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FitzMetrics? _fitzMetrics;

        public GetUsersController(IServiceScopeFactory scopeFactory, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet]
        [RequireDiscordAuth]
        public IActionResult GetUsers([FromQuery] string? query = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/users";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                if (page < 1)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Page must be greater than or equal to 1"
                    });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "PageSize must be between 1 and 100"
                    });
                }

                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var accountsQuery = db.Accounts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    var queryLower = query.ToLower();
                    accountsQuery = accountsQuery.Where(a => a.Username != null && a.Username.ToLower().Contains(queryLower));
                }

                var totalCount = accountsQuery.Count();

                var accounts = accountsQuery
                    .OrderBy(a => a.Username ?? string.Empty)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var response = new UsersResponse
                {
                    Users = accounts.Select(a => new UserResponse
                    {
                        Id = a.Id,
                        Username = a.Username
                    }).ToList(),
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

                return Ok(new ApiResponse<UsersResponse>
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
