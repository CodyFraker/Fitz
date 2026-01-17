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
    public class GetTopBalancesController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetTopBalancesController(BankService bankService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("top-balances")]
        [RequireDiscordAuth]
        public IActionResult GetTopBalances([FromQuery] int limit = 10)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/top-balances";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var accounts = _bankService.GetTopBeerBalances(limit);
                
                var response = new TopBalanceResponse
                {
                    Accounts = accounts.Select(a => new AccountBalanceResponse
                    {
                        Id = a.Id,
                        Username = a.Username,
                        Beer = a.Beer
                    }).ToList()
                };

                return Ok(new ApiResponse<TopBalanceResponse>
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
