using DSharpPlus.SlashCommands;

namespace Fitz.Api.Controllers.Music.PlayMusic.Domain;

public record PlayMusicCommand(ulong UserId, ulong GuildId, ulong? VoiceChannelId, string Song)
{
    public static PlayMusicCommand FromInteractionContext(InteractionContext ctx, string song)
    {
        return new PlayMusicCommand(
            UserId: ctx.User.Id,
            GuildId: ctx.Guild.Id,
            VoiceChannelId: ctx.Member.VoiceState?.Channel?.Id,
            Song: song
        );
    }
}
