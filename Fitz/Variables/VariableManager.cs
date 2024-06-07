using DSharpPlus;
using Fitz.Variables.Channels;
using Serilog;

namespace Fitz.Variables
{
    public static class VariableManager
    {
        public static void ApplyVariableScopes(DiscordClient dClient)
        {
            bool isDev = dClient.CurrentUser.Id == Users.DevelopmentBot;

            if (isDev)
            {
                Log.Warning("Development account detected, overriding variables.");
                DodeDuke.Settings = DodeDuke.DevelopmentSettings;
                Waterbear.MockFakeStub();
                Guilds.MockFakeStub();
                Roles.MockFakeStub();
                Users.MockFakeStub();
            }
        }
    }
}