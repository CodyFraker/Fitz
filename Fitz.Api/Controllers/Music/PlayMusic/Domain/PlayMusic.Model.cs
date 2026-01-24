namespace Fitz.Api.Controllers.Music.PlayMusic.Domain;

public record PlayMusicModel(
    ulong UserId,
    string Song,
    bool Success,
    string? Message)
{
    public static PlayMusicModel From(ulong userId, string song, bool success, string? message = null)
    {
        return new PlayMusicModel(
            UserId: userId,
            Song: song,
            Success: success,
            Message: message
        );
    }
}
