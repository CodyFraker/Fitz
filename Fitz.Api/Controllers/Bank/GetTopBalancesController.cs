using Fitz.Api.Attributes;
using Fitz.Api.Models.Responses;
using Fitz.Features.Bank;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Bank
{
    [ApiController]
    [Route("api/bank")]
    public class GetTopBalancesController : ControllerBase
    {
        private readonly BankService _bankService;

        public GetTopBalancesController(BankService bankService)
        {
            _bankService = bankService;
        }

        [HttpGet("top-balances")]
        [RequireDiscordAuth]
        public IActionResult GetTopBalances([FromQuery] int limit = 10)
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
    }
}
