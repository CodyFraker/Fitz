using Fitz.Api.Controllers.Admin.AdminSendMessage.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Admin.AdminSendMessage.Http;

[DisplayName("AdminSendMessageRequest")]
public record AdminSendMessageRequestDto
{
    [Required]
    public required ulong ChannelId { get; set; }

    [Required]
    public required string Message { get; set; }

    internal AdminSendMessageCommand ToCommand()
    {
        return AdminSendMessageCommand.From(this);
    }
}
