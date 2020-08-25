namespace Fitz.BackgroundServices
{
    using System;
    using System.IO;
    using Serilog.Core;
    using Serilog.Events;
    using Serilog.Formatting.Display;

    public class ConsoleSink : ILogEventSink
    {
        private readonly IFormatProvider formatProvider;
        private readonly string outputTemplate;

        public ConsoleSink(string outputTemplate, IFormatProvider formatProvider)
        {
            this.outputTemplate = outputTemplate;
            this.formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            // Skip bucket requests
            if (logEvent.MessageTemplate.Text.Contains("Request for bucket", StringComparison.Ordinal))
            {
                return;
            }

            MessageTemplateTextFormatter formatter = new MessageTemplateTextFormatter(this.outputTemplate, this.formatProvider);

            SetConsoleColor(logEvent.Level, logEvent.MessageTemplate.ToString());

            using (StringWriter sw = new StringWriter())
            {
                formatter.Format(logEvent, sw);
                Console.Write(sw.ToString());
            }

            Console.ResetColor();
        }

        private static void SetConsoleColor(LogEventLevel level, string message)
        {
            switch (level)
            {
                case LogEventLevel.Information:

                    switch (message)
                    {
                        case "[WIKI] Post Found":
                        case "[FORUMS] New Posting":
                        case "[HELPRACE] New Post!":
                        case "[REDDIT] New Post!":
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            break;
                    }

                    break;
                case LogEventLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogEventLevel.Error:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                case LogEventLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    break;
                default: // Debug and Verbose
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
        }
    }
}