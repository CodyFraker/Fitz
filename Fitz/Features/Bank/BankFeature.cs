using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Bank.AddBalance.Discord;
using Fitz.Features.Bank.GetBalance.Discord;
using Fitz.Variables;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Fitz.Features.Bank
{
    public class BankFeature : Feature
    {
        private readonly DiscordClient _client;
        private SlashCommandsExtension _slashCommands;
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
            _client = client ?? throw new ArgumentNullException(nameof(client));
            try
            {
                _slashCommands = client.GetSlashCommands();
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting SlashCommands: {ex.Message}");
                // Don't throw here, we'll handle the null case in Enable()
            }
            _getBalanceAdapter = getBalanceAdapter ?? throw new ArgumentNullException(nameof(getBalanceAdapter));
            _addBalanceAdapter = addBalanceAdapter ?? throw new ArgumentNullException(nameof(addBalanceAdapter));
            _adminBalanceAdapter = adminBalanceAdapter ?? throw new ArgumentNullException(nameof(adminBalanceAdapter));
        }

        public override Task Enable()
        {
            // Check if _slashCommands is null
            if (_slashCommands == null)
            {
                Log.Warning("SlashCommands is null in BankFeature.Enable(). Attempting to get it again.");
                try
                {
                    _slashCommands = _client.GetSlashCommands();
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to get SlashCommands in Enable(): {ex.Message}");
                    return Task.CompletedTask; // Return early to avoid NullReferenceException
                }
                
                if (_slashCommands == null)
                {
                    Log.Error("SlashCommands is still null after retry. Cannot register commands.");
                    return Task.CompletedTask; // Return early to avoid NullReferenceException
                }
            }

            try
            {
                // Register slash commands
                _slashCommands.RegisterCommands<GetBalanceAdapter>(Guilds.Waterbear);
                _slashCommands.RegisterCommands<AddBalanceAdapter>(Guilds.Waterbear);
                _slashCommands.RegisterCommands<AdminBalanceAdapter>(Guilds.Waterbear);
                Log.Information("Successfully registered BankFeature slash commands");
            }
            catch (Exception ex)
            {
                Log.Error($"Error registering BankFeature slash commands: {ex.Message}");
            }

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