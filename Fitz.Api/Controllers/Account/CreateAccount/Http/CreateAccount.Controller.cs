using Fitz.Api.Controllers.Account.CreateAccount.Domain;
using Fitz.Api.Controllers.Account.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account.CreateAccount.Http
{
    [ApiController]
    [Route("api/account")]
    public class CreateAccountController(CreateAccountFacade createAccountFacade, ILogger<CreateAccountController> logger) : ControllerBase
    {
        private readonly CreateAccountFacade _createAccountFacade = createAccountFacade;
        private readonly ILogger<CreateAccountController> _logger = logger;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAccountRequestDto request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Account creation request received. AccountId: {AccountId}, Username: {Username}", request.AccountId, request.Username);
            
            try
            {
                var command = request.ToCommand();

                var response = await _createAccountFacade.Execute(command, cancellationToken);

                var dto = CreateAccountResponseDto.From(response);

                _logger.LogInformation("Account created successfully via HTTP. AccountId: {AccountId}, Username: {Username}", request.AccountId, request.Username);
                
                return Ok(dto);
            }
            catch (AccountAlreadyExists ex)
            {
                _logger.LogWarning("Account creation failed - account already exists. AccountId: {AccountId}, Username: {Username}, DiscordId: {DiscordId}", request.AccountId, request.Username, ex.DiscordId);
                
                return Conflict(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Account creation failed - invalid argument. AccountId: {AccountId}, Username: {Username}, Error: {Error}", request.AccountId, request.Username, ex.Message);
                
                return BadRequest(ex.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Account creation failed - unexpected error. AccountId: {AccountId}, Username: {Username}", request.AccountId, request.Username);
                
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
