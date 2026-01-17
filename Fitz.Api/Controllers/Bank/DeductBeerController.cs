using Fitz.Api.Attributes;
using Fitz.Api.Models.Requests;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Fitz.Features.Bank.Models;
using Fitz.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class DeductBeerController : ControllerBase
    {
        private readonly BankService _bankService;
        private readonly FitzMetrics? _fitzMetrics;

        public DeductBeerController(BankService bankService, FitzMetrics? fitzMetrics = null)
        {
            _bankService = bankService;
            _fitzMetrics = fitzMetrics;
        }

        [HttpPost("deduct-beer")]
        [RequireDiscordAuth]
        public async Task<IActionResult> DeductBeer([FromBody] DeductBeerRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var endpoint = "/api/bank/deduct-beer";
            
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

                if (!Enum.TryParse<Reason>(request.Reason, out var reason))
                {
                    _fitzMetrics?.RecordApiError(endpoint, "validation_error");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid reason"
                    });
                }

                var result = await _bankService.DeductBeerFromUser(request.UserId, request.Amount, reason);

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
