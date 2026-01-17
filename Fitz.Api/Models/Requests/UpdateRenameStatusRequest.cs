using System.ComponentModel.DataAnnotations;
using Fitz.Features.Rename.Models;

namespace Fitz.Api.Models.Requests
{
    public class UpdateRenameStatusRequest
    {
        [Required]
        public RenameStatus Status { get; set; }
    }
}
