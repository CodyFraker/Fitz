using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Features.Accounts;

namespace Fitz.Api.Controllers.Admin.AdminUpdateFavorability.Domain;

public class AdminUpdateFavorabilityService(
    IAdminUpdateFavorability adminUpdateFavorability,
    AccountService accountService,
    ILogger<AdminUpdateFavorabilityService> logger)
{
    private readonly IAdminUpdateFavorability _adminUpdateFavorability = adminUpdateFavorability;
    private readonly AccountService _accountService = accountService;
    private readonly ILogger<AdminUpdateFavorabilityService> _logger = logger;

    public async Task<AdminUpdateFavorabilityModel> ExecuteAsync(AdminUpdateFavorabilityCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminUpdateFavorabilityService execution started. UserId: {UserId}, Favorability: {Favorability}", 
            command.UserId, command.Favorability);

        if (command.UserId == 0)
        {
            _logger.LogError("AdminUpdateFavorability validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        if (command.Favorability < 0 || command.Favorability > 100)
        {
            _logger.LogError("AdminUpdateFavorability validation failed - Favorability must be between 0 and 100. Favorability: {Favorability}", 
                command.Favorability);
            throw new ArgumentException("Favorability must be between 0 and 100.", nameof(command.Favorability));
        }

        var account = await _adminUpdateFavorability.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        var result = await _accountService.SetFavorabilityAsync(account, command.Favorability);
        if (!result.Success)
        {
            _logger.LogError("Failed to set favorability. Message: {Message}", result.Message);
            throw new InvalidOperationException(result.Message);
        }

        var model = AdminUpdateFavorabilityModel.From("Favorability updated successfully");

        _logger.LogInformation("AdminUpdateFavorabilityModel created successfully");

        return model;
    }
}
