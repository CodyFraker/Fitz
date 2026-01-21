using DSharpPlus;
using DSharpPlus.Entities;

namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;

public class AdminSendMessageService(
    DiscordClient discordClient,
    ILogger<AdminSendMessageService> logger)
{
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<AdminSendMessageService> _logger = logger;

    public async Task<AdminSendMessageModel> ExecuteAsync(AdminSendMessageCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("AdminSendMessageService execution started. ChannelId: {ChannelId}, MessageLength: {MessageLength}", 
            command.ChannelId, command.Message?.Length ?? 0);

        if (command.ChannelId == 0)
        {
            _logger.LogError("AdminSendMessage validation failed - Channel ID cannot be 0.");
            throw new ArgumentException("Channel ID cannot be 0.", nameof(command.ChannelId));
        }

        if (string.IsNullOrWhiteSpace(command.Message))
        {
            _logger.LogError("AdminSendMessage validation failed - Message cannot be empty.");
            throw new ArgumentException("Message cannot be empty.", nameof(command.Message));
        }

        var channel = await _discordClient.GetChannelAsync(command.ChannelId);
        if (channel == null)
        {
            _logger.LogWarning("Channel not found. ChannelId: {ChannelId}", command.ChannelId);
            throw new InvalidOperationException("Channel not found");
        }

        var message = await channel.SendMessageAsync(command.Message);

        var model = AdminSendMessageModel.From(message.Id, channel.Id, command.Message);

        _logger.LogInformation("AdminSendMessageModel created successfully. MessageId: {MessageId}, ChannelId: {ChannelId}", 
            message.Id, channel.Id);

        return model;
    }
}
