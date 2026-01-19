using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Api.Controllers.Account.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account.CreateAccount.Http
{
    [ApiController]
    [Route("api/account")]
    public class CreateAccountController(CreateAccountFacade createAccountFacade) : ControllerBase
    {
        private readonly CreateAccountFacade _createAccountFacade = createAccountFacade;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto request, CancellationToken cancellationToken = default)
        {
            try
            {
                var command = request.ToCommand();

                var response = await _createAccountFacade.Execute(command, cancellationToken);

                var dto = CreateAccountResponseDto.From(response);

                return Ok(dto);
            }
            catch (AccountAlreadyExists ex)
            {
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            { 
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
