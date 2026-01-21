namespace Fitz.Api.Controllers.Bank.AwardBonus.Domain;

public class AwardBonusFacade(AwardBonusService awardBonusService, ILogger<AwardBonusFacade> logger)
{
    private readonly AwardBonusService _awardBonusService = awardBonusService;
    private readonly ILogger<AwardBonusFacade> _logger = logger;

    public async Task<AwardBonusResponse> Execute(AwardBonusCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AwardBonusFacade execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var model = await _awardBonusService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AwardBonusService execution completed. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        var response = AwardBonusResponse.From(model);

        _logger.LogInformation("AwardBonusFacade execution completed successfully. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        return response;
    }
}
