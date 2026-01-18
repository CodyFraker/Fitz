namespace Fitz.Api.Models.Responses
{
    public class UsersResponse
    {
        public List<UserResponse> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserResponse
    {
        public ulong Id { get; set; }
        public string? Username { get; set; }
    }
}
