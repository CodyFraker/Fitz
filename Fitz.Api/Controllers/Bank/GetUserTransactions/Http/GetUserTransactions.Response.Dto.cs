using Fitz.Api.Controllers.Bank.GetUserTransactions.Domain;
using Fitz.Api.Models.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Fitz.Api.Controllers.Bank.GetUserTransactions.Http;

[DisplayName("GetUserTransactionsResponse")]
public record GetUserTransactionsResponseDto
{
    [Required]
    public required List<TransactionResponse> Transactions { get; set; }

    [Required]
    public required int TotalCount { get; set; }

    [Required]
    public required int Skip { get; set; }

    [Required]
    public required int Take { get; set; }

    public static GetUserTransactionsResponseDto From(GetUserTransactionsResponse response, int skip, int take)
    {
        return new GetUserTransactionsResponseDto
        {
            Transactions = response.Transactions.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Sender = t.Sender,
                Recipient = t.Recipient,
                Amount = t.Amount,
                Reason = t.Reason.ToString(),
                Timestamp = t.Timestamp
            }).ToList(),
            TotalCount = response.TotalCount,
            Skip = skip,
            Take = take
        };
    }
}
