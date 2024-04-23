using System.Text.RegularExpressions;

namespace Fitz.Utils
{
    public static class Convert
    {
        /// <summary>
        /// Capitalizes the first letter of a given string.
        /// </summary>
        /// <param name="input">The string to capitalize.</param>
        /// <returns>Capitalized string.</returns>
        public static string Capitalize(this string input)
        {
            return char.ToUpperInvariant(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// This method will drop all text after a given number of characters.
        /// </summary>
        /// <param name="input">The string you want to truncate.</param>
        /// <param name="maxLength">The amount of characters to limit the string to.</param>
        /// <returns>Truncated string.</returns>
        public static string Truncate(this string input, int maxLength)
        {
            // Shout out to StackOverflow
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.Length <= maxLength ? input : input.Substring(0, maxLength);
        }

        public static string RemoveRichText(string input)
        {
            return new Regex(@"<\/?(b|i|color|material|quad|size)(?:=.*?)?>").Replace(input, string.Empty).ToString();
        }
    }
}