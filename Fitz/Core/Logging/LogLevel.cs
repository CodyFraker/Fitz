namespace Fitz.Core.Logging
{
    /// <summary>
    /// Represents the severity level of a log message
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug-level messages
        /// </summary>
        Debug = 0,

        /// <summary>
        /// Informational messages
        /// </summary>
        Information = 1,

        /// <summary>
        /// Warning messages
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error messages
        /// </summary>
        Error = 3,

        /// <summary>
        /// Critical error messages
        /// </summary>
        Critical = 4
    }
} 