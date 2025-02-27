using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Bank.AddBalance.Discord;
using Fitz.Features.Bank.GetBalance.Discord;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Bank
{
    public class BankFeature : Feature
    {
        private readonly DiscordClient _client;
        private readonly SlashCommandsExtension _slashCommands;
        private readonly GetBalanceAdapter _getBalanceAdapter;
        private readonly AddBalanceAdapter _addBalanceAdapter;
        private readonly AdminBalanceAdapter _adminBalanceAdapter;

        public override string Name => "BankRework";
        public override string Description => "Manage beer currency and transactions";

        public BankFeature(
            DiscordClient client,
            GetBalanceAdapter getBalanceAdapter,
            AddBalanceAdapter addBalanceAdapter,
            AdminBalanceAdapter adminBalanceAdapter)
        {
            _client = client;
            _slashCommands = client.GetSlashCommands();
            _getBalanceAdapter = getBalanceAdapter;
            _addBalanceAdapter = addBalanceAdapter;
            _adminBalanceAdapter = adminBalanceAdapter;
        }

        public override Task Enable()
        {
            // Register slash commands
            _slashCommands.RegisterCommands<GetBalanceAdapter>(Guilds.Waterbear);
            _slashCommands.RegisterCommands<AddBalanceAdapter>(Guilds.Waterbear);
            _slashCommands.RegisterCommands<AdminBalanceAdapter>(Guilds.Waterbear);

            return Task.CompletedTask;
        }

        public override Task Disable()
        {
            // Unregister slash commands
            // _slashCommands.UnregisterCommands<GetBalanceAdapter>(Guilds.Waterbear);
            // _slashCommands.UnregisterCommands<AddBalanceAdapter>(Guilds.Waterbear);
            // _slashCommands.UnregisterCommands<AdminBalanceAdapter>(Guilds.Waterbear);

            return Task.CompletedTask;
        }
    }
} 