using DSharpPlus;
using Serilog;

namespace Fitz.Variables
{
    public static class VariableManager
    {
        public static void ApplyVariableScopes(DiscordClient dClient)
        {
            bool isDev = dClient.CurrentUser.Id == Users.Dodecuplet;

            if (isDev)
            {
                Log.Warning("Development account detected, overriding variables.");
                Channels.DodeDuke.Settings = Channels.DodeDuke.Settings;
                Guilds.MockFakeStub();
                Fitz.Variables.Users.MockFakeStub();
            }
        }
    }
}