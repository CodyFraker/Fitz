using DSharpPlus.SlashCommands;

namespace Fitz.Api.Controllers.Music.StopMusic.Domain;

public record StopMusicCommand(ulong UserId, ulong GuildId, ulong? VoiceChannelId)
{
    public static StopMusicCommand FromInteractionContext(InteractionContext ctx)
    {
        return new StopMusicCommand(
            UserId: ctx.User.Id,
            GuildId: ctx.Guild.Id,
            VoiceChannelId: ctx.Member.VoiceState?.Channel?.Id
        );
    }
}
