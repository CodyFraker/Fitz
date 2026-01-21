namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;

public record AdminSendMessageResponse(
    ulong MessageId,
    ulong ChannelId,
    string Message)
{
    public static AdminSendMessageResponse From(AdminSendMessageModel model)
    {
        return new AdminSendMessageResponse(MessageId: model.MessageId, ChannelId: model.ChannelId, Message: model.Message);
    }
}
