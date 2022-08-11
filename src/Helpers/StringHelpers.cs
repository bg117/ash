using System.Text;

namespace OpenProject.ApplicationShell.Helpers;

/// <summary>
///     Methods to help with string formatting and manipulation..
/// </summary>
internal static class StringHelpers
{
    /// <summary>
    ///     Converts <paramref name="byteCount" /> to a human-readable string.
    /// </summary>
    /// <param name="byteCount">The integer to process.</param>
    /// <returns>A human-readable string that describes the size of a file.</returns>
    internal static string BytesToString(long byteCount)
    {
        var suffix = new[]
        {
            "B", "K", "M", "G", "T", "P", "E"
        }; // long runs out around EB

        if (byteCount == 0)
            return "0" + suffix[0];

        var bytes = Math.Abs(byteCount);
        var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1000)));
        var num = Math.Round(bytes / Math.Pow(1000, place), 1);

        return Math.Sign(byteCount) * num + suffix[place];
    }

    /// <summary>
    ///     Splits the string by the delimiter (outside quotes) into an array.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <param name="delimiter">The string to split the string on.</param>
    /// <param name="stringEscape">Escape character for when a delimiter occurs inside quotation marks.</param>
    /// <returns></returns>
    internal static IEnumerable<string> SplitOutsideQuotes(
        this string str,
        string delimiter = " ",
        char stringEscape = '\\')
    {
        const char quote = '"';

        var sb = new StringBuilder(str.Length);
        var counter = 0;

        while (counter < str.Length)
            // if starts with delimiter if so read ahead to see if matches
            if (delimiter[0] == str[counter] &&
                delimiter.SequenceEqual(ReadNext(str, counter,
                    delimiter.Length)))
            {
                yield return sb.ToString();

                sb.Clear();
                counter +=
                    delimiter.Length; // move the counter past the delimiter
            }

            // if we hit a quote read until we hit another quote or end of string
            else if (str[counter] == quote)
            {
                sb.Append(str[counter++]);
                while (counter < str.Length &&
                       str[counter] != quote)
                {
                    if (str[counter] == stringEscape)
                        sb.Append(str[counter++]);

                    if (counter < str.Length)
                        sb.Append(str[counter++]);
                }

                // if not end of string then we hit a quote add the quote
                if (counter < str.Length)
                    sb.Append(str[counter++]);
            }
            else
            {
                sb.Append(str[counter++]);
            }

        if (sb.Length > 0)
            yield return sb.ToString();

        static IEnumerable<char> ReadNext(string str,
            int currentPosition,
            int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (currentPosition + i >= str.Length)
                    yield break;

                yield return str[currentPosition + i];
            }
        }
    }
}