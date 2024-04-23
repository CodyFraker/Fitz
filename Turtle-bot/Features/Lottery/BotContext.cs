﻿using Fitz.Features.Lottery.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitz.Core.Contexts
{
    public partial class BotContext : DbContext
    {
        public DbSet<Drawing> Drawing { get; set; }
        public DbSet<Ticket> Ticket { get; set; }
    }
}