using Fitz.Api.Controllers.Polls.Exceptions;

namespace Fitz.Api.Controllers.Admin.AdminDeletePoll.Domain;

public class AdminDeletePollService(
    IAdminDeletePoll adminDeletePoll,
    ILogger<AdminDeletePollService> logger)
{
    private readonly IAdminDeletePoll _adminDeletePoll = adminDeletePoll;
    private readonly ILogger<AdminDeletePollService> _logger = logger;

    public async Task<AdminDeletePollModel> ExecuteAsync(AdminDeletePollCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminDeletePollService execution started. Id: {Id}", command.Id);

        if (command.Id <= 0)
        {
            _logger.LogError("AdminDeletePoll validation failed - Id must be greater than 0. Id: {Id}", command.Id);
            throw new ArgumentException("Id must be greater than 0.", nameof(command.Id));
        }

        var poll = await _adminDeletePoll.FindPollByIdAsync(command.Id, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. Id: {Id}", command.Id);
            throw new PollNotFound(command.Id);
        }

        await _adminDeletePoll.DeletePollAsync(command.Id, cancellationToken);

        var model = AdminDeletePollModel.From($"Poll {command.Id} deleted successfully");

        _logger.LogInformation("AdminDeletePollModel created successfully. Id: {Id}", command.Id);

        return model;
    }
}
