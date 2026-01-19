namespace Fitz.Api.Controllers.Polls.PostPollToPending.Domain;

public class PostPollToPendingFacade(PostPollToPendingService postPollToPendingService, ILogger<PostPollToPendingFacade> logger)
{
    private readonly PostPollToPendingService _postPollToPendingService = postPollToPendingService;
    private readonly ILogger<PostPollToPendingFacade> _logger = logger;

    public async Task<PostPollToPendingResponse> Execute(PostPollToPendingCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("PostPollToPendingFacade execution started. PollId: {PollId}", command.PollId);

        var model = await _postPollToPendingService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("PostPollToPendingService execution completed. PollId: {PollId}, MessageId: {MessageId}", command.PollId, model.Poll.MessageId);

        var response = PostPollToPendingResponse.From(model);

        _logger.LogInformation("PostPollToPendingFacade execution completed successfully. PollId: {PollId}, MessageId: {MessageId}", command.PollId, model.Poll.MessageId);

        return response;
    }
}
