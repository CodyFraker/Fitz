using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Net;
using Fitz.Features.Accounts.Models;

namespace Fitz.Features.Accounts.Create.Discord;

public class CreateAccountResponse
{
    public HttpStatusCode StatusCode { get; set; }

    public string? Message { get; set; }

    public Account? Account { get; set; }
}