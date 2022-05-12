using System.Text.RegularExpressions;

namespace OpenProject.ASH;

internal static class ConsoleExtensions
{
    /// <summary>
    /// Colored <see cref="Console.WriteLine(string?)"/>.
    /// </summary>
    /// <param name="stream">The stream to use (Console.Out, Console.Error)</param>
    /// <param name="s">The string to print.</param>
    /// <param name="fgs">The list of foreground colors that will be applied to text surrounded with square brackets [].</param>
    /// <remarks>
    /// "Stolen" (it's OUR code anyway) from Stack Overflow.
    /// </remarks>
    internal static void ColoredWriteLine(TextWriter stream, string? s, params ConsoleColor[] fgs)
    {
        ColoredWrite(stream, s, fgs);
        stream.WriteLine();
    }

    /// <summary>
    /// Colored <see cref="Console.Write(string?)"/>.
    /// </summary>
    /// <param name="stream">The stream to use (Console.Out, Console.Error)</param>
    /// <param name="s">The string to print.</param>
    /// <param name="fgs">The list of foreground colors that will be applied to text surrounded with square brackets [].</param>
    /// <remarks>
    /// "Stolen" (it's OUR code anyway) from Stack Overflow.
    /// </remarks>
    internal static void ColoredWrite(TextWriter stream, string? s, params ConsoleColor[] fgs)
    {
        if (!(stream == Console.Out || stream == Console.Error)) // check if stream is not a System.Console TextWriter stream.
            throw new InvalidOperationException($"{nameof(stream)} only accepts System.Console streams.");

        if (s == null)
            return;

        var pieces = Regex.Split(s, @"(\[[^\]]*\])"); // extract [text]

        for (int i = 0, j = 0; i < pieces.Length; i++)
        {
            var piece = pieces[i];

            if (piece.StartsWith("[") && piece.EndsWith("]"))
            {
                Console.ForegroundColor = fgs[j++]; // next foreground color
                piece = piece[1..^1]; // remove the square brackets
            }

            stream.Write(piece); // write to stream
            Console.ResetColor();
        }
    }
}
