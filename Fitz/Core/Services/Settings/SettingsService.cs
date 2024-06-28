using Fitz.Core.Contexts;
using Fitz.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fitz.Core.Services.Settings
{
    public sealed class SettingsService(IServiceScopeFactory scopeFactory)
    {
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;

        #region Create Settings

        public async Task<Result> CreateBaseSettings()
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings != null)
                {
                    return new Result(false, "Settings already exist.", null);
                }
                else
                {
                    settings = new Models.Settings();
                    settings.LotteryDuration = 7;
                    settings.BaseLotteryPool = 36;
                    settings.LotteryPoolRollover = true;
                    settings.TicketCost = 1;
                    settings.MaxTickets = 128;
                    settings.AccountCreationBonusAmount = 128;
                    settings.BaseHappyHourAmount = 6;
                    settings.RenameBaseCost = 6;
                    settings.PollApprovedBonus = 24;
                    settings.PollSubmittedPenalty = 36;
                    settings.PollDeclinedPenalty = 0;
                    settings.PollVote = 12;
                    settings.PollCreatorTip = 6;
                    settings.MaxPendingPolls = 10;

                    db.Settings.Add(settings);
                    await db.SaveChangesAsync();
                    return new Result(true, "Base settings created.", settings);
                }
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed creating base settings. Exception message: {ex.Message}", null);
            }
        }

        #endregion Create Settings

        #region Set Lottery Duration

        public async Task<Result> SetLotteryDuration(int days)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (days > 365)
                {
                    return new Result(false, "The maximum lottery duration is 365 days.", null);
                }
                if (days < 0)
                {
                    return new Result(false, "The lottery duration must be longer than a single day.", null);
                }

                settings.LotteryDuration = days;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set lottery duration to {days} day(s).", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting lottery duration. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Lottery Duration

        #region Set Base Lottery Pool

        public async Task<Result> SetBaseLotteryPool(int pool)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (pool < 0)
                {
                    return new Result(false, "The base lottery pool must be a positive number.", null);
                }

                settings.BaseLotteryPool = pool;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set base lottery pool to {pool}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting base lottery pool. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Base Lottery Pool

        #region Set Lottery Pool Rollover

        public async Task<Result> SetLotteryPoolRollover(bool rollover)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                settings.LotteryPoolRollover = rollover;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set lottery pool rollover to {rollover}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting lottery pool rollover. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Lottery Pool Rollover

        #region Set Ticket Cost

        public async Task<Result> SetTicketCost(int cost)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (cost < 0)
                {
                    return new Result(false, "The ticket cost must be a positive number.", null);
                }

                settings.TicketCost = cost;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set ticket cost to {cost}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting ticket cost. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Ticket Cost

        #region Set Max Tickets

        public async Task<Result> SetMaxTickets(int maxTickets)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (maxTickets < 0)
                {
                    return new Result(false, "The maximum tickets must be a positive number.", null);
                }

                settings.MaxTickets = maxTickets;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set max tickets to {maxTickets}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting max tickets. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Max Tickets

        #region Set Happy Hour Base Amount

        public async Task<Result> SetHappyHourBaseAmount(int amount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (amount < 0)
                {
                    return new Result(false, "The happy hour base amount must be a positive number.", null);
                }

                settings.BaseHappyHourAmount = amount;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set happy hour base amount to {amount}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting happy hour base amount. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Happy Hour Base Amount

        #region Set Account Creation Bonus Amount

        public async Task<Result> SetAccountCreationBonusAmount(int amount)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (amount < 0)
                {
                    return new Result(false, "The account creation bonus amount must be a positive number.", null);
                }

                settings.AccountCreationBonusAmount = amount;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set account creation bonus amount to {amount}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting account creation bonus amount. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Account Creation Bonus Amount

        #region Set Rename Base Cost

        public async Task<Result> SetRenameBaseCost(int cost)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

                Models.Settings settings = db.Settings.FirstOrDefault();

                if (settings == null)
                {
                    await this.CreateBaseSettings();
                }

                if (cost < 0)
                {
                    return new Result(false, "The rename base cost must be a positive number.", null);
                }

                settings.RenameBaseCost = cost;
                db.Update(settings);
                await db.SaveChangesAsync();

                return new Result(true, $"Set rename base cost to {cost}.", settings);
            }
            catch (Exception ex)
            {
                return new Result(false, $"Failed setting rename base cost. Exception message: {ex.Message}", null);
            }
        }

        #endregion Set Rename Base Cost

        #region Get Settings

        public Models.Settings GetSettings()
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            using BotContext db = scope.ServiceProvider.GetRequiredService<BotContext>();

            Models.Settings settings = db.Settings.FirstOrDefault();

            if (settings == null)
            {
                this.CreateBaseSettings();
            }

            return db.Settings.FirstOrDefault();
        }

        #endregion Get Settings
    }
}