namespace Fitz.Core.Discord
{
    public enum LogConsoleSettings
    {
        /// <summary>
        /// Unknown channel
        /// </summary>
        None = 0,

        /// <summary>
        /// <see cref="Fitz.Variables.Channels.Waterbear.Exceptions"/>
        /// </summary>
        Console = 1,

        /// <summary>
        /// <see cref="Fitz.Variables.Channels.Waterbear.Commands"/>
        /// </summary>
        Commands = 2,

        /// <summary>
        /// <see cref="Fitz.Variables.Channels.Waterbear.Jobs"/>
        /// </summary>
        Jobs = 3,

        /// <summary>
        /// <see cref="Fitz.Variables.Channels.Waterbear.Exceptions"/>
        /// </summary>
        RoleEdits = 4,

        /// <summary>
        /// <see cref="Fitz.Variables.Channels.Waterbear.Exceptions"/>
        /// </summary>
        UserInfo = 5,

        /// <summary>
        /// <see cref="Fitz.Variables.Channels.Waterbear.LotteryLog"/>
        /// </summary>
        LotteryLog = 6,
    }
}