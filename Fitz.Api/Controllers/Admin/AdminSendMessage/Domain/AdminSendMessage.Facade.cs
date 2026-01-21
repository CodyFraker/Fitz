namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;

public class AdminSendMessageFacade(AdminSendMessageService adminSendMessageService, ILogger<AdminSendMessageFacade> logger)
{
    private readonly AdminSendMessageService _adminSendMessageService = adminSendMessageService;
    private readonly ILogger<AdminSendMessageFacade> _logger = logger;

    public async Task<AdminSendMessageResponse> Execute(AdminSendMessageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AdminSendMessageFacade execution started. ChannelId: {ChannelId}", command.ChannelId);

        var model = await _adminSendMessageService.ExecuteAsync(command, cancellationToken);

        _logger.LogInformation("AdminSendMessageService execution completed. MessageId: {MessageId}, ChannelId: {ChannelId}", 
            model.MessageId, model.ChannelId);

        var response = AdminSendMessageResponse.From(model);

        _logger.LogInformation("AdminSendMessageFacade execution completed successfully. MessageId: {MessageId}, ChannelId: {ChannelId}", 
            model.MessageId, model.ChannelId);

        return response;
    }
}
