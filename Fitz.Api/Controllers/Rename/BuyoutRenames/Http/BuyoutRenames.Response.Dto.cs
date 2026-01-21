using Fitz.Api.Controllers.Rename.BuyoutRenames.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Rename.BuyoutRenames.Http;

[DisplayName("BuyoutRenamesResponse")]
public record BuyoutRenamesResponseDto
{
    [Required]
    public required int RenamesUpdated { get; set; }

    [Required]
    public required string Message { get; set; }

    public static BuyoutRenamesResponseDto From(BuyoutRenamesResponse response)
    {
        return new BuyoutRenamesResponseDto
        {
            RenamesUpdated = response.RenamesUpdated,
            Message = response.Message
        };
    }
}
