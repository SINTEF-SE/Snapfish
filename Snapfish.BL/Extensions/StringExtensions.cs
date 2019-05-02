using System;

namespace Snapfish.BL.Extensions
{
    public static class StringExtensions
    {
        public static string GetUntilOrEmpty(this string text, string stopAt = ",")
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);
                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation);
                }
            }

            return string.Empty;
        }

        public static string Between(this string @this, string from = null, string until = null, StringComparison comparison = StringComparison.InvariantCulture)
        {
            int fromLength = (from ?? string.Empty).Length;
            int startIndex = !string.IsNullOrEmpty(from)
                ? @this.IndexOf(from, comparison) + fromLength
                : 0;

            if (startIndex < fromLength)
            {
                throw new ArgumentException("from: Failed to find an instance of the first anchor");
            }

            int endIndex = !string.IsNullOrEmpty(until)
                ? @this.IndexOf(until, startIndex, comparison)
                : @this.Length;

            if (endIndex < 0)
            {
                throw new ArgumentException("until: Failed to find an instance of the last anchor");
            }

            string subString = @this.Substring(startIndex, endIndex - startIndex);
            return subString;
        }
    }
}