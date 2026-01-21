namespace Fitz.Api.Controllers.Auth.ExchangeToken.Domain;

public class ExchangeTokenFacade(ExchangeTokenService exchangeTokenService, ILogger<ExchangeTokenFacade> logger)
{
    private readonly ExchangeTokenService _exchangeTokenService = exchangeTokenService;
    private readonly ILogger<ExchangeTokenFacade> _logger = logger;

    public async Task<ExchangeTokenResponse> Execute(ExchangeTokenCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("ExchangeTokenFacade execution started. RedirectUri: {RedirectUri}", command.RedirectUri);

        var model = await _exchangeTokenService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("ExchangeTokenService execution completed");

        var response = ExchangeTokenResponse.From(model);

        _logger.LogInformation("ExchangeTokenFacade execution completed successfully");

        return response;
    }
}
