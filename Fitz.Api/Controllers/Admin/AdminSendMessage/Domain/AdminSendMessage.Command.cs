using Fitz.Api.Controllers.Admin.AdminSendMessage.Http;

namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;

public record AdminSendMessageCommand(ulong ChannelId, string Message)
{
    public static AdminSendMessageCommand From(AdminSendMessageRequestDto request)
    {
        return new AdminSendMessageCommand(ChannelId: request.ChannelId, Message: request.Message);
    }
}
