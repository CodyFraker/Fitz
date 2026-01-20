using Fitz.Api.Attributes;
using Fitz.Api.Controllers.Account.GetAccount.Domain;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Fitz.Api.Controllers.Account.GetAccount.Http
{
    [ApiController]
    [Route("api/account")]
    public class GetAccountController(GetAccountFacade getAccountFacade, ILogger<GetAccountController> logger) : ControllerBase
    {
        private readonly GetAccountFacade _getAccountFacade = getAccountFacade;
        private readonly ILogger<GetAccountController> _logger = logger;

        [HttpGet("{userId}")]
        [RequireDiscordAuth]
        [RequireOwnData]
        public async Task<IActionResult> GetAccount(ulong userId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Account retrieval request received. UserId: {UserId}", userId);

            try
            {
                var command = GetAccountCommand.From(userId);

                var response = await _getAccountFacade.Execute(command, cancellationToken);

                var dto = GetAccountResponseDto.From(response);

                _logger.LogInformation("Account retrieved successfully via HTTP. UserId: {UserId}, Username: {Username}", userId, dto.Username);

                return Ok(new ApiResponse<GetAccountResponseDto>
                {
                    Success = true,
                    Data = dto
                });
            }
            catch (AccountNotFound ex)
            {
                _logger.LogWarning("Account retrieval failed - account not found. UserId: {UserId}", ex.UserId);

                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Account retrieval failed - invalid argument. UserId: {UserId}, Error: {Error}", userId, ex.Message);

                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Account retrieval failed - unexpected error. UserId: {UserId}", userId);

                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}
