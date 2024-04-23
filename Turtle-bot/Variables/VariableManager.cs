using DSharpPlus;
using Serilog;

namespace Fitz.Variables
{
    public static class VariableManager
    {
        public static void ApplyVariableScopes(DiscordClient dClient)
        {
            bool isDev = dClient.CurrentUser.Id == Users.Ruby;

            if (isDev)
            {
                Log.Warning("Development account detected, overriding variables.");
                Channels.DodeDuke.Settings = Channels.DodeDuke.Settings;
                Fitz.Variables.Guilds.Guilds.MockFakeStub();
                Fitz.Variables.Users.MockFakeStub();
            }
        }
    }
}