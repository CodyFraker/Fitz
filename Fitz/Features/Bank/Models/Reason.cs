namespace Fitz.Features.Bank.Models
{
    public enum Reason
    {
        /// <summary>
        /// Whenever someone creates an accounut they get beer
        /// </summary>
        AccountCreationBonus = 1,

        /// <summary>
        /// Awarded or given beer
        /// </summary>
        Bonus = 2,

        /// <summary>
        /// When someone gives beer to another.
        /// </summary>
        Donated = 3,

        /// <summary>
        /// Whenever someone buys a ticket for the lottery
        /// </summary>
        Lotto = 4,

        /// <summary>
        /// Whenever someone wins the lottery.
        /// </summary>
        LottoWin = 5,

        /// <summary>
        /// Whenever someone renames another user.
        /// </summary>
        Rename = 6,

        /// <summary>
        /// Whenever someone plays music.
        /// </summary>
        MusicPlay = 7,

        /// <summary>
        /// Whenever someone skips someone else's music.
        /// </summary>
        MusicSkip = 8,

        /// <summary>
        /// Whenever a user is in a voice channel during happy hour.
        /// </summary>
        HappyHour = 9,

        /// <summary>
        /// Whenever a user submits a poll.
        /// </summary>
        PollSubmitted = 10,

        /// <summary>
        /// Whenever a user submitted poll is approved.
        /// </summary>
        PollApproved = 11,

        /// <summary>
        /// Whenever a user submitted poll is declined.
        /// </summary>
        PollDeclined = 12,

        /// <summary>
        /// Whenever a user votes on a poll.
        /// </summary>
        PollVote = 13,

        PollCreatorTip = 14
    }
}