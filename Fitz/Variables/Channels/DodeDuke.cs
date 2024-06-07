using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fitz.Variables.Channels
{
    public static class DodeDuke
    {
        /// <summary>
        /// Bot Testing Grounds
        /// https://discord.com/channels/196820438398140417/366417664920256523
        /// </summary>
        public static ulong ground0 { get; set; } = 366417664920256523;

        /// <summary>
        /// Settings channels for enabling and disabling features.
        /// https://discord.com/channels/196820438398140417/1049375707962294313
        /// </summary>
        public static ulong Settings { get; set; } = 1066464641905078332;

        /// <summary>
        /// Settings channels for enabling and disabling features.
        /// https://discord.com/channels/196820438398140417/1049375893346320435
        /// </summary>
        public static ulong DevelopmentSettings { get; set; } = 1066464688499597372;

        /// <summary>
        /// Channel for bot owners/trusted users.
        /// https://discord.com/channels/196820438398140417/1049375870772580412
        /// </summary>
        public static ulong BotMods { get; set; } = 1066464735664554045;

        /// <summary>
        /// Where do we send exceptions?
        /// https://discord.com/channels/196820438398140417/1049375923364954223
        /// </summary>
        public static ulong Exceptions { get; set; } = 1066464756380221580;

        public static ulong Commands { get; set; } = 1066465514655858698;

        /// <summary>
        /// Where do we send role edit notifications?
        /// https://discord.com/channels/196820438398140417/1248654353833918596
        /// </summary>
        public static ulong RoleEdits { get; set; } = 1248654353833918596;

        /// <summary>
        /// Where do we send user event notifications?
        /// https://discord.com/channels/196820438398140417/1049376097894138009
        /// </summary>
        public static ulong UserEvents { get; set; } = 1066464786537254912;

        /// <summary>
        /// Where do we log jobs ran?
        /// https://discord.com/channels/196820438398140417/1049376002217885757
        /// </summary>
        public static ulong Jobs { get; set; } = 1066464867848048810;

        public static ulong AccountLog { get; set; } = 1247948886480261271;

        public static ulong LotteryLog { get; set; } = 1232351979750031371;

        public static ulong Transactions { get; set; } = 1232354188735021147;

        public static ulong PollLog { get; set; } = 1247990296181080084;

        public static ulong RenameLog { get; set; } = 1247957910827569173;
    }
}