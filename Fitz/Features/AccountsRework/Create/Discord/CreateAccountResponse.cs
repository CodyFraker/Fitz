using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Net;

namespace Fitz.Features.AccountsRework.Create.Discord;

public class CreateAccountResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Message { get; set; }

    public Account? Account { get; set; }
}