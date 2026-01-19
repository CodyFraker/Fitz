namespace Fitz.Api.Controllers.Polls.GetPollsWithDetails.Domain;

public class GetPollsWithDetailsFacade(GetPollsWithDetailsService getPollsWithDetailsService, ILogger<GetPollsWithDetailsFacade> logger)
{
    private readonly GetPollsWithDetailsService _getPollsWithDetailsService = getPollsWithDetailsService;
    private readonly ILogger<GetPollsWithDetailsFacade> _logger = logger;

    public async Task<GetPollsWithDetailsResponse> Execute(GetPollsWithDetailsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetPollsWithDetailsFacade execution started");

        var model = await _getPollsWithDetailsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("GetPollsWithDetailsService execution completed. TotalCount: {TotalCount}, Returned: {Returned}", 
            model.TotalCount, model.Polls.Count);

        var response = GetPollsWithDetailsResponse.From(model);

        _logger.LogInformation("GetPollsWithDetailsFacade execution completed successfully. TotalCount: {TotalCount}, Returned: {Returned}", 
            response.TotalCount, response.Polls.Count);

        return response;
    }
}
