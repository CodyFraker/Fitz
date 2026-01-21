using Fitz.Variables;

namespace Fitz.Api.Controllers.Admin.GetUsersWithFavorability.Domain;

public class GetUsersWithFavorabilityService(
    IGetUsersWithFavorability getUsersWithFavorability,
    ILogger<GetUsersWithFavorabilityService> logger)
{
    private readonly IGetUsersWithFavorability _getUsersWithFavorability = getUsersWithFavorability;
    private readonly ILogger<GetUsersWithFavorabilityService> _logger = logger;

    public async Task<GetUsersWithFavorabilityModel> ExecuteAsync(GetUsersWithFavorabilityCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetUsersWithFavorabilityService execution started. Query: {Query}, Skip: {Skip}, Take: {Take}, SortBy: {SortBy}, SortOrder: {SortOrder}", 
            command.Query, command.Skip, command.Take, command.SortBy, command.SortOrder);

        if (command.Skip < 0)
        {
            _logger.LogError("GetUsersWithFavorability validation failed - Skip must be greater than or equal to 0. Skip: {Skip}", command.Skip);
            throw new ArgumentException("Skip must be greater than or equal to 0.", nameof(command.Skip));
        }

        if (command.Take <= 0 || command.Take > 100)
        {
            _logger.LogError("GetUsersWithFavorability validation failed - Take must be between 1 and 100. Take: {Take}", command.Take);
            throw new ArgumentException("Take must be between 1 and 100.", nameof(command.Take));
        }

        var botAccount = await _getUsersWithFavorability.FindAccountByIdAsync(Fitz.Variables.Users.Fitz, cancellationToken);
        int botBeer = botAccount != null ? Math.Max(botAccount.Beer, 1) : 1;

        var (accounts, totalCount) = await _getUsersWithFavorability.GetUsersAsync(
            command.Query,
            command.Skip,
            command.Take,
            command.SortBy,
            command.SortOrder,
            cancellationToken);

        var model = GetUsersWithFavorabilityModel.From(accounts, totalCount, botBeer);

        _logger.LogInformation("GetUsersWithFavorabilityModel created successfully. TotalCount: {TotalCount}, Returned: {Returned}", 
            totalCount, accounts.Count);

        return model;
    }
}
