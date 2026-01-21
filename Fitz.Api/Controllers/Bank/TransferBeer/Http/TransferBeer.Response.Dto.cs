using Fitz.Api.Controllers.Bank.TransferBeer.Domain;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.TransferBeer.Http;

[DisplayName("TransferBeerResponse")]
public record TransferBeerResponseDto
{
    [Required]
    public required ulong SenderId { get; set; }

    [Required]
    public required ulong RecipientId { get; set; }

    [Required]
    public required int Amount { get; set; }

    [Required]
    public required int SenderNewBalance { get; set; }

    [Required]
    public required int RecipientNewBalance { get; set; }

    public static TransferBeerResponseDto From(TransferBeerResponse response)
    {
        return new TransferBeerResponseDto
        {
            SenderId = response.SenderId,
            RecipientId = response.RecipientId,
            Amount = response.Amount,
            SenderNewBalance = response.SenderNewBalance,
            RecipientNewBalance = response.RecipientNewBalance
        };
    }
}
