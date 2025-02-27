using DSharpPlus;
using DSharpPlus.SlashCommands;
using Fitz.Core.Services.Features;
using Fitz.Features.Polls.Create.Discord;
using Fitz.Features.Polls.Update.Discord;
using Fitz.Features.Polls.Vote.Discord;
using Fitz.Variables;
using System.Threading.Tasks;

namespace Fitz.Features.Polls
{
    public sealed class PollsFeature : Feature
    {
        private readonly DiscordClient _client;
        private readonly SlashCommandsExtension _slashCommands;
        private readonly CreatePollAdapter _createPollAdapter;
        private readonly UpdatePollAdapter _updatePollAdapter;
        private readonly VoteAdapter _voteAdapter;

        public override string Name => "Polls";
        public override string Description => "Create and vote on polls.";

        public PollsFeature(
            DiscordClient client,
            CreatePollAdapter createPollAdapter,
            UpdatePollAdapter updatePollAdapter,
            VoteAdapter voteAdapter)
        {
            _client = client;
            _slashCommands = client.GetSlashCommands();
            _createPollAdapter = createPollAdapter;
            _updatePollAdapter = updatePollAdapter;
            _voteAdapter = voteAdapter;
        }

        public override Task Enable()
        {
            // Register slash commands
            _slashCommands.RegisterCommands<CreatePollAdapter>(Guilds.Waterbear);
            _slashCommands.RegisterCommands<UpdatePollAdapter>(Guilds.Waterbear);
            _slashCommands.RegisterCommands<VoteAdapter>(Guilds.Waterbear);

            return Task.CompletedTask;
        }

        public override Task Disable()
        {
            // Since DSharpPlus doesn't have a direct UnregisterCommands method,
            // we would need to implement a custom solution or handle this differently
            // For now, we'll leave this as a placeholder
            
            return Task.CompletedTask;
        }
    }
} 