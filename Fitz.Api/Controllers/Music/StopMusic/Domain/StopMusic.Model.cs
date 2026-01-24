namespace Fitz.Api.Controllers.Music.StopMusic.Domain;

public record StopMusicModel(
    ulong UserId,
    bool Success,
    string? Message)
{
    public static StopMusicModel From(ulong userId, bool success, string? message = null)
    {
        return new StopMusicModel(
            UserId: userId,
            Success: success,
            Message: message
        );
    }
}
