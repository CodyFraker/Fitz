namespace Fitz.Api.Models.Responses
{
    public class CurrentUserResponse
    {
        public ulong Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
    }
}
