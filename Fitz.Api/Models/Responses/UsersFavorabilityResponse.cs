using System.Collections.Generic;

namespace Fitz.Api.Models.Responses
{
    public class UsersFavorabilityResponse
    {
        public List<UserFavorabilityResponse> Users { get; set; }
        public int TotalCount { get; set; }
    }
}
