namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;

public record AdminSendMessageModel(
    ulong MessageId,
    ulong ChannelId,
    string Message)
{
    public static AdminSendMessageModel From(ulong messageId, ulong channelId, string message)
    {
        return new AdminSendMessageModel(MessageId: messageId, ChannelId: channelId, Message: message);
    }
}
