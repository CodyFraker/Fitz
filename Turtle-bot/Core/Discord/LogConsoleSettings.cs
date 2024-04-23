namespace Fitz.Core.Discord
{
    public enum LogConsoleSettings
    {
        /// <summary>
        /// Unknown channel
        /// </summary>
        None = 0,

        /// <summary>
        /// <see cref="BloonChannels.Console"/>
        /// </summary>
        Console = 1,

        /// <summary>
        /// <see cref="BloonChannels.Commands"/>
        /// </summary>
        Commands = 2,

        /// <summary>
        /// <see cref="BloonChannels.Jobs"/>
        /// </summary>
        Jobs = 3,

        /// <summary>
        /// <see cref="BloonChannels.RoleEdits"/>
        /// </summary>
        RoleEdits = 4,

        /// <summary>
        /// <see cref="BloonChannels.SBGUserInfo"/>
        /// </summary>
        UserInfo = 5,
    }
}
