using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.ModalCommands;
using DSharpPlus.ModalCommands.Attributes;
using DSharpPlus.SlashCommands;
using Fitz.Api.Controllers.Account.Exceptions;
using Fitz.Api.Controllers.Polls.CreatePoll.Domain;
using Fitz.Api.Controllers.Polls.Exceptions;
using Fitz.Api.Controllers.Polls.PostPollToPending.Domain;
using Fitz.Database.Entities;
using Fitz.Features.Polls;
using Fitz.Features.Polls.Models;
using Fitz.Variables.Emojis;
using System.Security.Cryptography;

namespace Fitz.Api.Controllers.Polls.CreatePoll.Discord;

[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public sealed class CreatePollModalCommands(
    CreatePollFacade createPollFacade,
    PostPollToPendingFacade postPollToPendingFacade,
    ICreatePoll createPoll,
    PollService pollService,
    DiscordClient discordClient,
    ILogger<CreatePollModalCommands> logger) : ModalCommandModule
{
    private readonly CreatePollFacade _createPollFacade = createPollFacade;
    private readonly PostPollToPendingFacade _postPollToPendingFacade = postPollToPendingFacade;
    private readonly ICreatePoll _createPoll = createPoll;
    private readonly PollService _pollService = pollService;
    private readonly DiscordClient _discordClient = discordClient;
    private readonly ILogger<CreatePollModalCommands> _logger = logger;

    #region Number

    [ModalCommand("gen_number")]
    public async Task GenerateNumberPoll(ModalContext ctx, string genNumberPollTitle, string pollOptions)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Number poll modal submitted. UserId: {UserId}, Username: {Username}", userId, username);

        int uniqueId = GenerateUniqueId();

        string[] answerOptions = pollOptions.Split(',');
        answerOptions = answerOptions.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        List<PollOptionsEntity> pollOptionsList = new List<PollOptionsEntity>();

        if (answerOptions.Length > 10 || answerOptions.Length <= 1)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"You need at least 2 options but no more than 10. You provided {answerOptions.Length} option(s).")
                .AsEphemeral(true));
            return;
        }

        for (int i = 0; i < answerOptions.Length; i++)
        {
            pollOptionsList.Add(new PollOptionsEntity
            {
                Answer = answerOptions[i].Trim(),
                EmojiName = GetNumberEmojiName(i),
                EmojiId = DiscordEmoji.FromName(ctx.Client, GetNumberEmojiName(i)).Id,
            });
        }

        var settings = await _createPoll.GetSettingsAsync(CancellationToken.None);
        if (settings == null)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Failed to retrieve settings. Please try again later.")
                .AsEphemeral(true));
            return;
        }

        DiscordButtonComponent acceptBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"number_poll_confirm_{uniqueId}", "Confirm", false);
        DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"number_poll_cancel_{uniqueId}", "Cancel", false);

        var previewEmbed = GeneratePreviewEmbed(ctx, genNumberPollTitle, pollOptionsList, PollTypeEnum.Number);

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
            .AddEmbed(previewEmbed)
            .AddComponents(cancelBtn, acceptBtn).AsEphemeral(true));

        _discordClient.ComponentInteractionCreated += async (s, e) =>
        {
            if (e.User.Id != userId) return;

            if (e.Id == $"number_poll_confirm_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Submitting number poll..."));

                var result = await SubmitPoll(ctx, genNumberPollTitle, pollOptionsList, PollTypeEnum.Number);
                if (result.Success)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I will evaluate if the poll is worthy of posting. If so, you will gain beer."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Submission Failed. {result.Message}"));
                }
            }
            else if (e.Id == $"number_poll_cancel_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
            }
        };
    }

    #endregion Number

    #region Color

    [ModalCommand("generate_color_poll")]
    public async Task GenerateColorPoll(ModalContext ctx, string colorPollTitle, string pollOptions)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Color poll modal submitted. UserId: {UserId}, Username: {Username}", userId, username);

        int uniqueId = GenerateUniqueId();

        string[] answerOptions = pollOptions.Split(',');
        answerOptions = answerOptions.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        List<PollOptionsEntity> pollOptionsList = new List<PollOptionsEntity>();

        if (answerOptions.Length > 9 || answerOptions.Length < 1)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent($"You can only have a maximum of 9 options for color polls. You provided {answerOptions.Length}")
                .AsEphemeral(true));
            return;
        }

        for (int i = 0; i < answerOptions.Length; i++)
        {
            pollOptionsList.Add(new PollOptionsEntity
            {
                Answer = answerOptions[i].Trim(),
                EmojiName = GetColorEmojiName(i),
                EmojiId = DiscordEmoji.FromName(ctx.Client, GetColorEmojiName(i)).Id,
            });
        }

        var settings = await _createPoll.GetSettingsAsync(CancellationToken.None);
        if (settings == null)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Failed to retrieve settings. Please try again later.")
                .AsEphemeral(true));
            return;
        }

        DiscordButtonComponent acceptBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"color_poll_confirm_{uniqueId}", "Confirm", false);
        DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"color_poll_cancel_{uniqueId}", "Cancel", false);

        var previewEmbed = GeneratePreviewEmbed(ctx, colorPollTitle, pollOptionsList, PollTypeEnum.Color);

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
            .AddEmbed(previewEmbed)
            .AddComponents(cancelBtn, acceptBtn).AsEphemeral(true));

        _discordClient.ComponentInteractionCreated += async (s, e) =>
        {
            if (e.User.Id != userId) return;

            if (e.Id == $"color_poll_confirm_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Submitting color poll.."));

                var result = await SubmitPoll(ctx, colorPollTitle, pollOptionsList, PollTypeEnum.Color);
                if (result.Success)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Created."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {result.Message}"));
                }
            }
            else if (e.Id == $"color_poll_cancel_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
            }
        };
    }

    #endregion Color

    #region Yes or No

    [ModalCommand("gen_yesno")]
    public async Task GenerateYesNoPoll(ModalContext ctx, string yesnoPollTitle)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Yes/No poll modal submitted. UserId: {UserId}, Username: {Username}", userId, username);

        int uniqueId = GenerateUniqueId();

        DiscordButtonComponent acceptBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"yesno_poll_confirm_{uniqueId}", "Confirm", false);
        DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"yesno_poll_cancel_{uniqueId}", "Cancel", false);

        List<PollOptionsEntity> pollOptions =
        [
            new PollOptionsEntity
            {
                Answer = "Yes",
                EmojiName = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes).Name,
                EmojiId = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.Yes).Id,
            },
            new PollOptionsEntity
            {
                Answer = "No",
                EmojiName = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No).Name,
                EmojiId = DiscordEmoji.FromGuildEmote(ctx.Client, PollEmojis.No).Id,
            },
        ];

        var settings = await _createPoll.GetSettingsAsync(CancellationToken.None);
        if (settings == null)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Failed to retrieve settings. Please try again later.")
                .AsEphemeral(true));
            return;
        }

        var previewEmbed = GeneratePreviewEmbed(ctx, yesnoPollTitle, pollOptions, PollTypeEnum.YesOrNo);

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
            .AddEmbed(previewEmbed)
            .AddComponents(cancelBtn, acceptBtn).AsEphemeral(true));

        _discordClient.ComponentInteractionCreated += async (s, e) =>
        {
            if (e.User.Id != userId) return;

            if (e.Id == $"yesno_poll_confirm_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));

                var result = await SubmitPoll(ctx, yesnoPollTitle, pollOptions, PollTypeEnum.YesOrNo);
                if (result.Success)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Created."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {result.Message}"));
                }
            }
            else if (e.Id == $"yesno_poll_cancel_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
            }
        };
    }

    #endregion Yes or No

    #region This Or That

    [ModalCommand("gen_thisorthat")]
    public async Task GenerateThisOrThatPoll(ModalContext ctx, string thisOrThatPollTitle, string This, string That)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("This or That poll modal submitted. UserId: {UserId}, Username: {Username}", userId, username);

        int uniqueId = GenerateUniqueId();

        DiscordButtonComponent acceptBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"thisorthat_poll_confirm_{uniqueId}", "Confirm", false);
        DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"thisorthat_poll_cancel_{uniqueId}", "Cancel", false);

        List<PollOptionsEntity> pollOptions =
        [
            new PollOptionsEntity
            {
                Answer = This,
                EmojiName = ":point_left:",
                EmojiId = DiscordEmoji.FromName(ctx.Client, ":point_left:").Id,
            },
            new PollOptionsEntity
            {
                Answer = That,
                EmojiName = ":point_right:",
                EmojiId = DiscordEmoji.FromName(ctx.Client, ":point_right:").Id,
            },
        ];

        var settings = await _createPoll.GetSettingsAsync(CancellationToken.None);
        if (settings == null)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Failed to retrieve settings. Please try again later.")
                .AsEphemeral(true));
            return;
        }

        var previewEmbed = GeneratePreviewEmbed(ctx, thisOrThatPollTitle, pollOptions, PollTypeEnum.ThisOrThat);

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
            .AddEmbed(previewEmbed)
            .AddComponents(cancelBtn, acceptBtn).AsEphemeral(true));

        _discordClient.ComponentInteractionCreated += async (s, e) =>
        {
            if (e.User.Id != userId) return;

            if (e.Id == $"thisorthat_poll_confirm_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Posting poll.."));

                var result = await SubmitPoll(ctx, thisOrThatPollTitle, pollOptions, PollTypeEnum.ThisOrThat);
                if (result.Success)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Created."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {result.Message}"));
                }
            }
            else if (e.Id == $"thisorthat_poll_cancel_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll not created."));
            }
        };
    }

    #endregion This Or That

    #region Hot Take

    [ModalCommand("gen_hottake")]
    public async Task GenerateHotTakePoll(ModalContext ctx, string PollTitle)
    {
        var userId = ctx.User.Id;
        var username = ctx.User.Username;

        _logger.LogInformation("Hot Take poll modal submitted. UserId: {UserId}, Username: {Username}", userId, username);

        int uniqueId = GenerateUniqueId();

        DiscordButtonComponent acceptBtn = new DiscordButtonComponent(DiscordButtonStyle.Success, $"hottake_poll_confirm_{uniqueId}", "Confirm", false);
        DiscordButtonComponent cancelBtn = new DiscordButtonComponent(DiscordButtonStyle.Danger, $"hottake_poll_cancel_{uniqueId}", "Cancel", false);

        List<PollOptionsEntity> pollOptions = new List<PollOptionsEntity>();
        pollOptions.Add(new PollOptionsEntity
        {
            Answer = "Agree",
            EmojiName = ":fire:",
            EmojiId = DiscordEmoji.FromName(ctx.Client, ":fire:").Id,
        });
        pollOptions.Add(new PollOptionsEntity
        {
            Answer = "Shit Take",
            EmojiName = ":poop:",
            EmojiId = DiscordEmoji.FromName(ctx.Client, ":poop:").Id,
        });

        var settings = await _createPoll.GetSettingsAsync(CancellationToken.None);
        if (settings == null)
        {
            await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                .WithContent("Failed to retrieve settings. Please try again later.")
                .AsEphemeral(true));
            return;
        }

        var previewEmbed = GeneratePreviewEmbed(ctx, PollTitle, pollOptions, PollTypeEnum.HotTake);

        await ctx.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
            .WithContent($"Here is what the poll will look like. If everything looks good, hit 'Confirm' and you will be charged {settings.PollSubmittedPenalty} beer for the poll submission. Clicking cancel will *NOT* submit the poll and you will be forced to start over.")
            .AddEmbed(previewEmbed)
            .AddComponents(cancelBtn, acceptBtn).AsEphemeral(true));

        _discordClient.ComponentInteractionCreated += async (s, e) =>
        {
            if (e.User.Id != userId) return;

            if (e.Id == $"hottake_poll_confirm_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Submitting poll.."));

                var result = await SubmitPoll(ctx, PollTitle, pollOptions, PollTypeEnum.HotTake);
                if (result.Success)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll Submitted."));
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Poll Creation Failed. {result.Message}"));
                }
            }
            else if (e.Id == $"hottake_poll_cancel_{uniqueId}")
            {
                await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Poll creation canceled"));
            }
        };
    }

    #endregion Hot Take

    #region Helper Methods

    private int GenerateUniqueId()
    {
        int uniqueId = 0;
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            byte[] data = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                rng.GetBytes(data);
                uniqueId = BitConverter.ToInt32(data, 0);
                uniqueId = Math.Abs(uniqueId);
            }
        }
        return uniqueId;
    }

    private string GetNumberEmojiName(int index)
    {
        return index switch
        {
            0 => ":one:",
            1 => ":two:",
            2 => ":three:",
            3 => ":four:",
            4 => ":five:",
            5 => ":six:",
            6 => ":seven:",
            7 => ":eight:",
            8 => ":nine:",
            9 => ":keycap_ten:",
            _ => ":one:"
        };
    }

    private string GetColorEmojiName(int index)
    {
        return index switch
        {
            0 => ":blue_circle:",
            1 => ":green_circle:",
            2 => ":orange_circle:",
            3 => ":purple_circle:",
            4 => ":red_circle:",
            5 => ":yellow_circle:",
            6 => ":brown_circle:",
            7 => ":black_circle:",
            8 => ":white_circle:",
            _ => ":blue_circle:"
        };
    }

    private DiscordEmbed GeneratePreviewEmbed(ModalContext ctx, string question, List<PollOptionsEntity> pollOptions, PollTypeEnum pollType)
    {
        var tempPoll = new PollEntity
        {
            Question = question,
            Type = pollType
        };

        return _pollService.GeneratePollEmbed(_discordClient, tempPoll, pollOptions);
    }

    private async Task<(bool Success, string Message)> SubmitPoll(ModalContext ctx, string question, List<PollOptionsEntity> pollOptions, PollTypeEnum pollType)
    {
        try
        {
            var command = new CreatePollCommand(
                AccountId: ctx.User.Id,
                MessageId: 0,
                Question: question,
                Type: pollType,
                Options: pollOptions.Select(o => new PollOptionCommand(
                    Answer: o.Answer,
                    EmojiName: o.EmojiName,
                    EmojiId: o.EmojiId
                )).ToList()
            );

            var createResponse = await _createPollFacade.Execute(command, CancellationToken.None);

            var postCommand = PostPollToPendingCommand.From(createResponse.Id);
            await _postPollToPendingFacade.Execute(postCommand, CancellationToken.None);

            _logger.LogInformation("Poll created and posted successfully. PollId: {PollId}, UserId: {UserId}", createResponse.Id, ctx.User.Id);

            return (true, "Poll created successfully");
        }
        catch (AccountNotFound ex)
        {
            _logger.LogWarning("Poll submission failed - account not found. UserId: {UserId}", ex.UserId);
            return (false, "You need to run `/signup` before you can create a poll.");
        }
        catch (InsufficientBeerException ex)
        {
            _logger.LogWarning("Poll submission failed - insufficient beer. UserId: {UserId}, Required: {Required}, Current: {Current}", 
                ctx.User.Id, ex.RequiredAmount, ex.CurrentBalance);
            return (false, ex.Message);
        }
        catch (MaxPendingPollsReachedException ex)
        {
            _logger.LogWarning("Poll submission failed - max pending polls reached. UserId: {UserId}, Current: {Current}, Max: {Max}", 
                ctx.User.Id, ex.CurrentCount, ex.MaxCount);
            return (false, ex.Message);
        }
        catch (InvalidPollOptionCountException ex)
        {
            _logger.LogWarning("Poll submission failed - invalid option count. UserId: {UserId}, PollType: {PollType}, Actual: {Actual}, Expected: {Min}-{Max}", 
                ctx.User.Id, ex.PollType, ex.ActualCount, ex.MinCount, ex.MaxCount);
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Poll submission failed - unexpected error. UserId: {UserId}", ctx.User.Id);
            return (false, $"An error occurred while creating the poll: {ex.Message}");
        }
    }

    #endregion Helper Methods
}
