using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class TransferBeerController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly FitzMetrics? _fitzMetrics;

        public TransferBeerController(BankService bankService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("transfer")]
        [RequireDiscordAuth]
        public async Task<IActionResult> TransferBeer([FromBody] TransferBeerRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/transfer";
            
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

                var result = await _bankService.TransferBeer(request.SenderId, request.RecipientId, request.Amount);

                if (!result.Success)
                {
                    _fitzMetrics?.RecordApiError(endpoint, "business_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Data
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
