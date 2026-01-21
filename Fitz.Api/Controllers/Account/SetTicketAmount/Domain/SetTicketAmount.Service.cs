using Fitz.Api.Controllers.Account.Exceptions;

namespace Fitz.Api.Controllers.Account.SetTicketAmount.Domain;

public class SetTicketAmountService(ISetTicketAmount setTicketAmount, ILogger<SetTicketAmountService> logger)
{
    private readonly ISetTicketAmount _setTicketAmount = setTicketAmount;
    private readonly ILogger<SetTicketAmountService> _logger = logger;

    public async Task<SetTicketAmountModel> ExecuteAsync(SetTicketAmountCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SetTicketAmountService execution started. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        if (command.UserId == 0)
        {
            _logger.LogError("SetTicketAmount validation failed - User ID cannot be 0.");
            throw new ArgumentException("User ID cannot be 0.", nameof(command.UserId));
        }

        if (command.Amount < 0)
        {
            _logger.LogError("SetTicketAmount validation failed - Amount cannot be negative. Amount: {Amount}", command.Amount);
            throw new ArgumentException("Amount cannot be negative.", nameof(command.Amount));
        }

        var account = await _setTicketAmount.FindAccountByIdAsync(command.UserId, cancellationToken);
        if (account == null)
        {
            _logger.LogWarning("Account not found. UserId: {UserId}", command.UserId);
            throw new AccountNotFound(command.UserId);
        }

        account.SubscribeTickets = command.Amount;
        await _setTicketAmount.UpdateAccountAsync(account, cancellationToken);

        var model = SetTicketAmountModel.From(account, command.Amount);

        _logger.LogInformation("SetTicketAmountModel created successfully. UserId: {UserId}, Amount: {Amount}", command.UserId, command.Amount);

        return model;
    }
}
