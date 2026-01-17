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
    public class GetUserTransactionsController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly FitzMetrics? _fitzMetrics;

        public GetUserTransactionsController(BankService bankService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpGet("transactions/{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetUserTransactions(ulong userId)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/transactions";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                var transactions = _bankService.GetTransactions(userId);
                
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
