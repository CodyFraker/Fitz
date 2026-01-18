using System.ComponentModel.DataAnnotations;
using Fitz.Database.Entities;

namespace Fitz.Api.Models.Requests
{
    public class UpdateRenameStatusRequest
    {
        [Required]
        public RenameStatus Status { get; set; }
    }
}
