using System;

namespace Fitz.Api.Controllers.Users.GetUsers.Domain;

public class GetUsersService(IGetUsers getUsers, ILogger<GetUsersService> logger)
{
    private readonly IGetUsers _getUsers = getUsers;
    private readonly ILogger<GetUsersService> _logger = logger;

    public async Task<GetUsersModel> ExecuteAsync(GetUsersCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetUsersService execution started. Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
            command.Query, command.Page, command.PageSize);

        if (command.Page < 1)
        {
            _logger.LogError("GetUsers validation failed - Page must be greater than or equal to 1. Page: {Page}", command.Page);
            throw new ArgumentException("Page must be greater than or equal to 1.", nameof(command.Page));
        }

        if (command.PageSize < 1 || command.PageSize > 100)
        {
            _logger.LogError("GetUsers validation failed - PageSize must be between 1 and 100. PageSize: {PageSize}", command.PageSize);
            throw new ArgumentException("PageSize must be between 1 and 100.", nameof(command.PageSize));
        }

        var skip = (command.Page - 1) * command.PageSize;
        var (accounts, totalCount) = await _getUsers.GetUsersAsync(command.Query, skip, command.PageSize, cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)command.PageSize);

        var model = GetUsersModel.From(accounts, totalCount, command.Page, command.PageSize, totalPages);

        _logger.LogInformation("GetUsersModel created successfully. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}, TotalPages: {TotalPages}", 
            totalCount, command.Page, command.PageSize, totalPages);

        return model;
    }
}
