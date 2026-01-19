using DSharpPlus;
using DSharpPlus.Entities;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Polls;
using Fitz.Variables.Channels;
using Fitz.Variables.Emojis;

namespace Fitz.Api.Controllers.Polls.PostPollToPending.Domain;

public class PostPollToPendingService(
    IPostPollToPending postPollToPending,
    PollService pollService,
    DiscordClient discordClient,
    ILogger<PostPollToPendingService> logger)
{
    private readonly IPostPollToPending _postPollToPending = postPollToPending;
    private readonly PollService _pollService = pollService;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<PostPollToPendingService> _logger = logger;

    public async Task<PostPollToPendingModel> ExecuteAsync(PostPollToPendingCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("PostPollToPendingService execution started. PollId: {PollId}", command.PollId);

        var poll = await _postPollToPending.FindPollByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            _logger.LogWarning("Poll not found. PollId: {PollId}", command.PollId);
            throw new PollNotFound(command.PollId);
        }

        if (poll.MessageId != 0)
        {
            _logger.LogWarning("Poll already posted. PollId: {PollId}, MessageId: {MessageId}", command.PollId, poll.MessageId);
            throw new PollAlreadyPostedException();
        }

        if (poll.Status != PollStatusEnum.Pending)
        {
            _logger.LogWarning("Invalid poll status. PollId: {PollId}, Status: {Status}", command.PollId, poll.Status);
            throw new InvalidPollStatusException(PollStatusEnum.Pending.ToString(), poll.Status.ToString());
        }

        var pollOptions = await _postPollToPending.GetPollOptionsAsync(command.PollId, cancellationToken);
        if (pollOptions == null || pollOptions.Count == 0)
        {
            _logger.LogWarning("Poll has no options. PollId: {PollId}", command.PollId);
            throw new InvalidOperationException("Poll has no options");
        }

        var channel = await _discordClient.GetChannelAsync(Waterbear.PendingPolls);
        if (channel == null)
        {
            _logger.LogError("Pending polls channel not found");
            throw new InvalidOperationException("Pending polls channel not found");
        }

        var embed = _pollService.GeneratePollEmbed(_discordClient, poll, pollOptions);
        var pollMessage = await channel.SendMessageAsync(embed);

        try
        {
            await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(_discordClient, PollEmojis.Yes));
            await pollMessage.CreateReactionAsync(DiscordEmoji.FromGuildEmote(_discordClient, PollEmojis.No));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add approval reactions. PollId: {PollId}", command.PollId);
            throw new InvalidOperationException($"Failed to add approval reactions: {ex.Message}");
        }

        poll.MessageId = pollMessage.Id;
        await _postPollToPending.UpdatePollAsync(poll, cancellationToken);

        _logger.LogInformation("PostPollToPendingService execution completed. PollId: {PollId}, MessageId: {MessageId}", command.PollId, poll.MessageId);

        return PostPollToPendingModel.From(poll);
    }
}
