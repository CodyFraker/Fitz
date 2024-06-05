using DSharpPlus.SlashCommands;

namespace Fitz.Core.Models
{
    public enum SettingsAction
    {
        [ChoiceName("LotteryDuration")]
        LotteryDuration,

        [ChoiceName("LotteryPool")]
        LotteryPool,

        [ChoiceName("LotteryPoolRollover")]
        LotteryPoolRollover,

        [ChoiceName("TicketCost")]
        TicketCost,

        [ChoiceName("MaxTickets")]
        MaxTickets,

        [ChoiceName("BaseHappyHourAmount")]
        BaseHappyHourAmount,

        [ChoiceName("AccountCreationBonusAmount")]
        AccountCreationBonusAmount,

        [ChoiceName("RenameBaseCost")]
        RenameBaseCost
    }
}