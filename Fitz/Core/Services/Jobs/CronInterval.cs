namespace Fitz.Core.Services.Jobs
{
    public class CronInterval
    {
        public const string Every1Minute = "* * * * *";
        public const string Every5Minutes = "*/5 * * * *";
        public const string Every15Minutes = "*/15 * * * *";
        public const string Every25Minutes = "*/25 * * * *";
        public const string Every30Minutes = "*/30 * * * *";
        public const string EveryHour = "0 * * * *";
    }
}
