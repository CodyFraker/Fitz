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
        [RequireOwnData]
        public IActionResult GetUserTransactions(ulong userId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/transactions";
            
            _fitzMetrics?.RecordApiRequest(endpoint, "GET");
            
            try
            {
                if (take < 1 || take > 100)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Take must be between 1 and 100"
                    });
                }

                if (skip < 0)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Skip must be greater than or equal to 0"
                    });
                }

                var (transactions, totalCount) = _bankService.GetTransactions(userId, skip, take);
                
                var response = new TransactionsResponse
                {
                    Transactions = transactions.Select(t => new TransactionResponse
                    {
                        Id = t.Id,
                        Sender = t.Sender,
                        Recipient = t.Recipient,
                        Amount = t.Amount,
                        Reason = t.Reason.ToString(),
                        Timestamp = t.Timestamp
                    }).ToList(),
                    TotalCount = totalCount,
                    Skip = skip,
                    Take = take
                };

                return Ok(new ApiResponse<TransactionsResponse>
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
