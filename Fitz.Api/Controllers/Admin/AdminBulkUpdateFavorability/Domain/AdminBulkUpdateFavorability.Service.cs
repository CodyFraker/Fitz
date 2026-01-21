using Fitz.Features.Accounts;

namespace Fitz.Api.Controllers.Admin.AdminBulkUpdateFavorability.Domain;

public class AdminBulkUpdateFavorabilityService(
    IAdminBulkUpdateFavorability adminBulkUpdateFavorability,
    AccountService accountService,
    ILogger<AdminBulkUpdateFavorabilityService> logger)
{
    private readonly IAdminBulkUpdateFavorability _adminBulkUpdateFavorability = adminBulkUpdateFavorability;
    private readonly AccountService _accountService = accountService;
    private readonly ILogger<AdminBulkUpdateFavorabilityService> _logger = logger;

    public async Task<AdminBulkUpdateFavorabilityModel> ExecuteAsync(AdminBulkUpdateFavorabilityCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminBulkUpdateFavorabilityService execution started. UserIdsCount: {UserIdsCount}, Favorability: {Favorability}", 
            command.UserIds?.Length ?? 0, command.Favorability);

        if (command.UserIds == null || command.UserIds.Length == 0)
        {
            _logger.LogError("AdminBulkUpdateFavorability validation failed - UserIds array cannot be empty");
            throw new ArgumentException("UserIds array cannot be empty.", nameof(command.UserIds));
        }

        if (command.Favorability < 0 || command.Favorability > 100)
        {
            _logger.LogError("AdminBulkUpdateFavorability validation failed - Favorability must be between 0 and 100. Favorability: {Favorability}", 
                command.Favorability);
            throw new ArgumentException("Favorability must be between 0 and 100.", nameof(command.Favorability));
        }

        int successCount = 0;
        int failCount = 0;

        foreach (var userId in command.UserIds)
        {
            try
            {
                var account = await _adminBulkUpdateFavorability.FindAccountByIdAsync(userId, cancellationToken);
                if (account != null)
                {
                    var result = await _accountService.SetFavorabilityAsync(account, command.Favorability);
                    if (result.Success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failCount++;
                        _logger.LogWarning("Failed to update favorability for user. UserId: {UserId}, Message: {Message}", userId, result.Message);
                    }
                }
                else
                {
                    failCount++;
                    _logger.LogWarning("Account not found for user. UserId: {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                failCount++;
                _logger.LogError(ex, "Error updating favorability for user. UserId: {UserId}", userId);
            }
        }

        var message = $"Bulk update completed. Success: {successCount}, Failed: {failCount}";
        var model = AdminBulkUpdateFavorabilityModel.From(successCount, failCount, message);

        _logger.LogInformation("AdminBulkUpdateFavorabilityModel created successfully. SuccessCount: {SuccessCount}, FailCount: {FailCount}", 
            successCount, failCount);

        return model;
    }
}
