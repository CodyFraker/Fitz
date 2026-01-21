using Fitz.Api.Controllers.Bank.TransferBeer.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Http;

[DisplayName("TransferBeerRequest")]
public record TransferBeerRequestDto
{
    [Required]
    public required ulong SenderId { get; set; }

    [Required]
    public required ulong RecipientId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public required int Amount { get; set; }

    internal TransferBeerCommand ToCommand()
    {
        return TransferBeerCommand.From(this);
    }
}
