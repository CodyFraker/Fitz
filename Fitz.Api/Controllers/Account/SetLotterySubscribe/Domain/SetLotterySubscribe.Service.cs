using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;

public class SetLotterySubscribeService(ISetLotterySubscribe setLotterySubscribe, ILogger<SetLotterySubscribeService> logger)
{
    private readonly ISetLotterySubscribe _setLotterySubscribe = setLotterySubscribe;
    private readonly ILogger<SetLotterySubscribeService> _logger = logger;

    public async Task<SetLotterySubscribeModel> ExecuteAsync(SetLotterySubscribeCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SetLotterySubscribeService execution started. UserId: {UserId}, Subscribe: {Subscribe}", command.UserId, command.Subscribe);

        if (command.UserId == 0)
        {
            _logger.LogError("SetLotterySubscribe validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        var account = await _setLotterySubscribe.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        account.subscribeToLottery = command.Subscribe;
        await _setLotterySubscribe.UpdateAccountAsync(account, cancellationToken);

        var model = SetLotterySubscribeModel.From(account, command.Subscribe);

        _logger.LogInformation("SetLotterySubscribeModel created successfully. UserId: {UserId}, Subscribe: {Subscribe}", command.UserId, command.Subscribe);

        return model;
    }
}
