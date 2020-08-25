namespace Fitz.BackgroundServices
{
    using System;
    using Serilog;
    using Serilog.Configuration;
    using Serilog.Events;

    public static class FitzConsoleSinkExtensions
    {
        public static LoggerConfiguration FitzConsoleSink(
            this LoggerSinkConfiguration loggerConfiguration,
            string outputTemplate = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new ConsoleSink(outputTemplate, formatProvider), restrictedToMinimumLevel);
        }
    }
}