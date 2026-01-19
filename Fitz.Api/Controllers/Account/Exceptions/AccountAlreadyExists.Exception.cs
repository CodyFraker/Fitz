namespace Fitz.Api.Controllers.Account.Exceptions
{
    public class AccountAlreadyExists(ulong discordId, string discordName) 
        : Exception($"An account already exists. Discord ID: {discordId} Discord Name: {discordName}")
    {
        public readonly ulong DiscordId = discordId;
        public readonly string DiscordName = discordName;
    }
}
