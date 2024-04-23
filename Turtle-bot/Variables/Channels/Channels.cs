using System.Net.NetworkInformation;

namespace Fitz.Variables.Channels
{
    public static class Channels
    {
        /// <summary>
        /// CHANGE THIS
        /// A channel where the bot should log its contents to.
        /// </summary>
        public const ulong loggingChannel = 1066464756380221580;

        /// <summary>
        /// Settings channels for enabling and disabling features.
        /// </summary>
        public static ulong Settings { get; set; } = 1066464641905078332;

        public static ulong DevSettings { get; set; } = 1066464688499597372;

        /// <summary>
        /// Channel for bot owners/trusted users.
        /// </summary>
        public static ulong BotMods { get; set; } = 1066464735664554045;

        public static ulong LotteryInfo { get; set; } = 1232083050268069948;
    }
}