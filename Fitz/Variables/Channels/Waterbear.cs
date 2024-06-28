using System.Net.NetworkInformation;

namespace Fitz.Variables.Channels
{
    public static class Waterbear
    {
        /// <summary>
        /// Where the bot will dump the rules or other information into.
        /// https://discord.com/channels/1022879771526451240/1049433556339921006
        /// </summary>
        public static ulong Rules { get; private set; } = 1049433556339921006;

        public static ulong General { get; private set; } = 1022879772092669985;

        public static ulong Memes { get; private set; } = 1055301689487396956;

        public static ulong BotChannel { get; private set; } = 1066507610615975946;

        public static ulong Photography { get; private set; } = 1150803807215828993;

        public static ulong YoutubeVideos { get; private set; } = 1169846522423291904;

        public static ulong VoiceChatOne { get; private set; } = 1022879772092669986;

        public static ulong VoiceChatTwo { get; private set; } = 1027398959372705913;

        public static ulong VoiceChatThree { get; private set; } = 1240365194325131376;

        public static ulong Polls { get; private set; } = 1066465880671780936;

        public static ulong PendingPolls { get; private set; } = 1245940317513842728;

        public static ulong LotteryInfo { get; private set; } = 1232083050268069948;

        public static ulong WaterbearAdmins { get; private set; } = 1233144618854514788;

        public static void MockFakeStub()
        {
            Rules = DodeDuke.ground0;
            General = DodeDuke.ground0;
            Memes = DodeDuke.ground0;
            BotChannel = DodeDuke.ground0;
            Photography = DodeDuke.ground0;
            YoutubeVideos = DodeDuke.ground0;
            VoiceChatOne = DodeDuke.ground0;
            VoiceChatTwo = DodeDuke.ground0;
            VoiceChatThree = DodeDuke.ground0;
            Polls = DodeDuke.PollDev;
            PendingPolls = DodeDuke.PollDev;
            LotteryInfo = DodeDuke.LotteryDev;
            WaterbearAdmins = DodeDuke.BotMods;
        }
    }
}