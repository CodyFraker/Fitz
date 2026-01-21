using Fitz.Api.Controllers.Bank.GetTransactions.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.GetTransactions.Http;

[DisplayName("GetTransactionsResponse")]
public record GetTransactionsResponseDto
{
    [Required]
    public required List<TransactionResponse> Transactions { get; set; }

    public static GetTransactionsResponseDto From(GetTransactionsResponse response)
    {
        return new GetTransactionsResponseDto
        {
            Transactions = response.Transactions.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Sender = t.Sender,
                Recipient = t.Recipient,
                Amount = t.Amount,
                Reason = t.Reason.ToString(),
                Timestamp = t.Timestamp
            }).ToList()
        };
    }
}
