using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Fitz.Core.Contexts;
using Fitz.Utils;

namespace Bot.Features.FAQ.Models
{
    [Group("ar")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class FAQCommands : BaseCommandModule
    {
        private const int FaqsPerEmbed = 5;

        private readonly BotContext db;
        private readonly Manager faqManager;

        public FAQCommands(BotContext db, Manager faqManager)
        {
            this.db = db;
            this.faqManager = faqManager;
        }

        [Command("add")]
        [Description("Adds an faq response")]
        public async Task AddAsync(CommandContext ctx, string pattern, string message)
        {
            Faq faq = new Faq
            {
                Regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase),
                Message = message,
            };

            this.db.Faqs.Add(faq);

            await this.db.SaveChangesAsync();

            this.faqManager.AddFaq(faq);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("edit")]
        [Description("Edits an faq response")]
        public async Task EditAsync(CommandContext ctx, int id, string pattern, string message)
        {
            Faq faq = await this.db.Faqs
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (faq == null)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":interrobang:"));
                return;
            }

            faq.Regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            faq.Message = message;

            this.db.Faqs.Update(faq);

            await this.db.SaveChangesAsync();

            this.faqManager.ReplaceFaq(faq.Id, faq);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("remove")]
        [Description("Removes an faq response")]
        public async Task RemoveAsync(CommandContext ctx, int id)
        {
            Faq faq = await this.db.Faqs
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (faq == null)
            {
                await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":interrobang:"));
                return;
            }

            this.db.Faqs.Remove(faq);

            await this.db.SaveChangesAsync();

            this.faqManager.RemoveFaq(faq.Id);

            await ctx.Message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
        }

        [Command("list")]
        [Description("Retrieves the list of faq responses")]
        public async Task ListAsync(CommandContext ctx, int page = 1)
        {
            page = page < 1 ? 1 : page;

            int total = await this.db.Faqs.CountAsync();
            List<Faq> found = await this.db.Faqs
                .Skip((page - 1) * FaqsPerEmbed)
                .Take(FaqsPerEmbed)
                .ToListAsync();

            string description = found.Count == 0
                ? "PEBCAK!"
                : $"```diff\n{string.Join("\n\n", found.Select(x => $"+ (#{x.Id}): {x.Regex}\n{x.Message}"))}```".Truncate(2000);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(217, 187, 19),
                Timestamp = DateTime.UtcNow,
                Title = $"Current FAQ Auto Responses (Page {page}/{(int)Math.Ceiling((decimal)total / FaqsPerEmbed)})",
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "View pages with \".ar list <page>\"",
                },
            };

            await ctx.RespondAsync(embed);
        }
    }
}