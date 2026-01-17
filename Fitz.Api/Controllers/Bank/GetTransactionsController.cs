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
    public class GetTransactionsController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetTransactionsController(BankService bankService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("transactions")]
        [RequireDiscordAuth]
        public IActionResult GetTransactions([FromQuery] int take = 10)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/transactions";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var transactions = _bankService.GetTransactions(take);
                
                var response = transactions.Select(t => new TransactionResponse
                {
                    Id = t.Id,
                    Sender = t.Sender,
                    Recipient = t.Recipient,
                    Amount = t.Amount,
                    Reason = t.Reason.ToString(),
                    Timestamp = t.Timestamp
                }).ToList();

                return Ok(new ApiResponse<List<TransactionResponse>>
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
