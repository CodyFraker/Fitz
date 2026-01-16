using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class GetUserTransactionsController : ControllerBase
    {
        private readonly BankService _bankService;

        public GetUserTransactionsController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet("transactions/{userId}")]
        [RequireDiscordAuth]
        public IActionResult GetUserTransactions(ulong userId)
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
    }
}
