using DSharpPlus.Entities;
using Fitz.Core.Contexts;
using Fitz.Features.Accounts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Features.Rename
{
    public class RenameService
    {
        private readonly IServiceScopeFactory scopeFactory;

        public RenameService(IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
        }

        public async Task<Account> RenameUserAsync(DiscordUser user)
        {
            // Don't allow users to rename the bot.
            if (user.Username == "Fitz")
            {
                return null;
            }
        }
    }
}