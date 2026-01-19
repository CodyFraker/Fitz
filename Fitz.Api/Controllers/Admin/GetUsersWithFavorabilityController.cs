using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Database;
using Fitz.Features.Accounts;
using Fitz.Database.Entities;
using Fitz.Features.Accounts.Queries;
using Fitz.Metrics;
using Fitz.Variables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fitz.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/favorability")]
    public class GetUsersWithFavorabilityController : ControllerBase
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AccountService _accountService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetUsersWithFavorabilityController(IServiceScopeFactory scopeFactory, AccountService accountService, FitzMetrics? fitzMetrics = null)
        {
            _scopeFactory = scopeFactory;
            _accountService = accountService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("users")]
        [RequireDiscordAuth]
        [RequireAdmin]
        public IActionResult GetUsersWithFavorability(
            [FromQuery] string? query = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/admin/favorability/users";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                using var scope = _scopeFactory.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<BotContext>();

                var findAccountQuery = new FindAccountQuery(_scopeFactory);
                var botAccount = findAccountQuery.Execute(746797148263415989);
                int botBeer = botAccount != null ? Math.Max(botAccount.Beer, 1) : 1;

                var accountsQuery = db.Accounts.AsQueryable();

                if (!string.IsNullOrWhiteSpace(query))
                {
                    if (ulong.TryParse(query, out ulong userId))
                    {
                        accountsQuery = accountsQuery.Where(a => a.Id == userId);
                    }
                    else
                    {
                        accountsQuery = accountsQuery.Where(a => a.Username != null && a.Username.Contains(query));
                    }
                }

                var totalCount = accountsQuery.Count();

                IQueryable<Fitz.Database.Entities.AccountEntity> sortedQuery = sortBy?.ToLower() switch
                {
                    "favorability" => sortOrder?.ToLower() == "desc" 
                        ? accountsQuery.OrderByDescending(a => a.Favorability)
                        : accountsQuery.OrderBy(a => a.Favorability),
                    "beer" => sortOrder?.ToLower() == "desc"
                        ? accountsQuery.OrderByDescending(a => a.Beer)
                        : accountsQuery.OrderBy(a => a.Beer),
                    "username" => sortOrder?.ToLower() == "desc"
                        ? accountsQuery.OrderByDescending(a => a.Username)
                        : accountsQuery.OrderBy(a => a.Username),
                    _ => accountsQuery.OrderBy(a => a.Id)
                };

                var accounts = sortedQuery.Skip(skip).Take(take).ToList();

                var users = accounts.Select(account =>
                {
                    decimal beerRatio = (decimal)account.Beer / botBeer;
                    return new UserFavorabilityResponse
                    {
                        UserId = account.Id,
                        Username = account.Username ?? "Unknown",
                        Beer = account.Beer,
                        BotBeer = botAccount?.Beer ?? 0,
                        BeerRatio = beerRatio,
                        Favorability = account.Favorability,
                        CanUseCommands = account.Favorability > 0
                    };
                }).ToList();

                var response = new UsersFavorabilityResponse
                {
                    Users = users,
                    TotalCount = totalCount
                };

                return Ok(new ApiResponse<UsersFavorabilityResponse>
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
