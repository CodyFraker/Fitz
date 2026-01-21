namespace Fitz.Api.Controllers.Admin.AdminBuyFitzTickets.Domain;

public class AdminBuyFitzTicketsFacade(AdminBuyFitzTicketsService adminBuyFitzTicketsService, ILogger<AdminBuyFitzTicketsFacade> logger)
{
    private readonly AdminBuyFitzTicketsService _adminBuyFitzTicketsService = adminBuyFitzTicketsService;
    private readonly ILogger<AdminBuyFitzTicketsFacade> _logger = logger;

    public async Task<AdminBuyFitzTicketsResponse> Execute(AdminBuyFitzTicketsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminBuyFitzTicketsFacade execution started. Tickets: {Tickets}", command.Tickets);

        var model = await _adminBuyFitzTicketsService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminBuyFitzTicketsService execution completed. Message: {Message}", model.Message);

        var response = AdminBuyFitzTicketsResponse.From(model);

        _logger.LogInformation("AdminBuyFitzTicketsFacade execution completed successfully. Message: {Message}", model.Message);

        return response;
    }
}
