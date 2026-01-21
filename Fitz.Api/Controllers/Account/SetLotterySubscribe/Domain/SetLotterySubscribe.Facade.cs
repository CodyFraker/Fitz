namespace Fitz.Api.Controllers.Account.SetLotterySubscribe.Domain;

public class SetLotterySubscribeFacade(SetLotterySubscribeService setLotterySubscribeService, ILogger<SetLotterySubscribeFacade> logger)
{
    private readonly SetLotterySubscribeService _setLotterySubscribeService = setLotterySubscribeService;
    private readonly ILogger<SetLotterySubscribeFacade> _logger = logger;

    public async Task<SetLotterySubscribeResponse> Execute(SetLotterySubscribeCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SetLotterySubscribeFacade execution started. UserId: {UserId}, Subscribe: {Subscribe}", command.UserId, command.Subscribe);

        var model = await _setLotterySubscribeService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("SetLotterySubscribeService execution completed. UserId: {UserId}, Subscribe: {Subscribe}", command.UserId, command.Subscribe);

        var response = SetLotterySubscribeResponse.From(model);

        _logger.LogInformation("SetLotterySubscribeFacade execution completed successfully. UserId: {UserId}, Subscribe: {Subscribe}", command.UserId, command.Subscribe);

        return response;
    }
}
