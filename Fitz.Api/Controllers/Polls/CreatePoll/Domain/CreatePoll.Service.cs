using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Database.Entities;
using Fitz.Features.Bank;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Domain;

public class CreatePollService(ICreatePoll createPoll, BankService bankService, ILogger<CreatePollService> logger)
{
    private readonly ICreatePoll _createPoll = createPoll;
    private readonly BankService _bankService = bankService;
    private readonly ILogger<CreatePollService> _logger = logger;

    public async Task<CreatePollModel> ExecuteAsync(CreatePollCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("CreatePollService execution started. AccountId: {AccountId}, Type: {Type}", command.AccountId, command.Type);

        var account = await _createPoll.FindAccountByIdAsync(command.AccountId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. AccountId: {AccountId}", command.AccountId);
            throw new AccountNotFound(command.AccountId);
        }

        var settings = await _createPoll.GetSettingsAsync(cancellationToken);
        if (settings == null)
        {
            _logger.LogError("Failed to get settings");
            throw new InvalidOperationException("Failed to get settings");
        }

        var requiredBeer = settings.PollSubmittedPenalty + settings.PollDeclinedPenalty;
        if (account.Beer < requiredBeer)
        {
            _logger.LogWarning("Insufficient beer. AccountId: {AccountId}, Required: {Required}, Current: {Current}", 
                command.AccountId, requiredBeer, account.Beer);
            throw new InsufficientBeerException(requiredBeer, account.Beer);
        }

        var pendingPollsCount = await _createPoll.GetPendingPollsCountAsync(command.AccountId, cancellationToken);
        if (pendingPollsCount >= settings.MaxPendingPolls)
        {
            _logger.LogWarning("Max pending polls reached. AccountId: {AccountId}, Current: {Current}, Max: {Max}", 
                command.AccountId, pendingPollsCount, settings.MaxPendingPolls);
            throw new MaxPendingPollsReachedException(pendingPollsCount, settings.MaxPendingPolls);
        }

        ValidatePollOptions(command.Type, command.Options.Count);

        var poll = new PollEntity
        {
            AccountId = command.AccountId,
            MessageId = command.MessageId,
            Question = command.Question,
            Type = command.Type,
            Status = PollStatusEnum.Pending,
            SubmittedOn = DateTime.UtcNow
        };

        var createdPoll = await _createPoll.CreatePollAsync(poll, cancellationToken);

        await _bankService.UserSubmittedPollPenalty(command.AccountId);

        var pollOptions = command.Options.Select(o => new PollOptionsEntity
        {
            PollId = createdPoll.Id,
            Answer = o.Answer,
            EmojiName = o.EmojiName,
            EmojiId = o.EmojiId
        }).ToList();

        var createdOptions = await _createPoll.CreatePollOptionsAsync(createdPoll.Id, pollOptions, cancellationToken);

        _logger.LogInformation("CreatePollService execution completed. PollId: {PollId}", createdPoll.Id);

        return CreatePollModel.From(createdPoll, createdOptions);
    }

    private void ValidatePollOptions(PollTypeEnum pollType, int optionCount)
    {
        switch (pollType)
        {
            case PollTypeEnum.Number:
                if (optionCount < 2 || optionCount > 10)
                {
                    throw new InvalidPollOptionCountException("Number", 2, 10, optionCount);
                }
                break;

            case PollTypeEnum.Color:
                if (optionCount < 1 || optionCount > 9)
                {
                    throw new InvalidPollOptionCountException("Color", 1, 9, optionCount);
                }
                break;

            case PollTypeEnum.YesOrNo:
            case PollTypeEnum.ThisOrThat:
            case PollTypeEnum.HotTake:
                if (optionCount != 2)
                {
                    throw new InvalidPollOptionCountException(pollType.ToString(), 2, 2, optionCount);
                }
                break;
        }
    }
}
