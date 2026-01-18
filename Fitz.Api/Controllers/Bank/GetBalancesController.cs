using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class GetBalancesController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetBalancesController(BankService bankService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("balances")]
        [RequireDiscordAuth]
        public IActionResult GetBalances([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/balances";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var (accounts, totalCount) = _bankService.GetBalances(skip, take);
                
                var response = new BalancesResponse
                {
                    Accounts = accounts.Select(a => new AccountBalanceResponse
                    {
                        Id = a.Id,
                        Username = a.Username,
                        Beer = a.Beer
                    }).ToList(),
                    TotalCount = totalCount,
                    Skip = skip,
                    Take = take
                };

                return Ok(new ApiResponse<BalancesResponse>
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
