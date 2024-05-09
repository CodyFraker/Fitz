using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts;
using Fitz.Features.Accounts.Models;
using Fitz.Features.Bank;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fitz.Features.Rename.Models;
using Fitz.Core.Models;

namespace Fitz.Features.Rename
{
    public class RenameService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly AccountService accountService;
        private readonly BankService bankService;

        public RenameService(IServiceScopeFactory scopeFactory, AccountService accountService, BankService bankService)
        {
            this.scopeFactory = scopeFactory;
            this.accountService = accountService;
        }

        public async Task<Result> RenameUserAsync(DiscordUser affectedUser, DiscordUser requestedUser, string newName)
        {
            // Don't allow users to rename the bot.
            if (affectedUser.Username == "Fitz")
            {
                return new Result(false, "You can't rename the bot.", null);
            }

            // Don't allow users to rename themselves.
            if (affectedUser.Id == requestedUser.Id)
            {
                return new Result(false, "You can't rename yourself.", null);
            }

            // Check to see if the two users has an account.
            Account affectedUserAccount = accountService.FindAccount(affectedUser.Id);
            Account requestedUserAccount = accountService.FindAccount(requestedUser.Id);
            if (affectedUserAccount == null || requestedUserAccount == null)
            {
                return new Result(false, "One of the users doesn't have an account.", null);
            }

            Renames rename = new Renames()
            {
                OldName = affectedUser.Username,
                NewName = newName,
                AffectedUserId = affectedUser.Id,
                RequestedUserId = requestedUser.Id,
                Days = 6,
                Timestamp = DateTime.Now,
            };

            // Set the new name for the affected user.
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            db.Renames.Add(rename);
            await db.SaveChangesAsync();
            return new Result(true, "Successfully renamed user.", null);
        }
    }
}