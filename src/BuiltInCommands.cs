using System.Text.RegularExpressions;

namespace OpenProject.ASH;

/// <summary>
/// Built-in commands helper class. Exceptions are to be handled by the calling code.
/// </summary>
internal static class BuiltInCommands
{
    private static readonly HelpContext[] HelpContexts =
    {
        new() { Command = "help", Description = "Shows the name and description of every command.", Usage = "help [command]" },
        new() { Command = "echo", Description = "Prints the succeeding arguments to the console and a new line. Quotes won't be removed. They will be printed as-is.", Usage = "echo [text]" },
        new() { Command = "print", Description = "Formats and prints the text according to the format string. Quotes will be removed. Also un-escapes escape sequences (like \\n, \\r, etc.).", Usage = "print <format> [arg1, [arg2, [...]]" },
        new() { Command = "cd", Description = "Changes the current directory.", Usage = "cd <directory>" }
    };

    private enum PrintState
    {
        Default,
        Format
    }

    private enum FormatState
    {
        Default,
        VeryShort,
        Short,
        Long,
        VeryLong
    }

    internal static void Help(string? command = null)
    {
        if (command == null)
        {
            foreach (var ctx in HelpContexts)
                Console.WriteLine($"{ctx.Command}: {ctx.Description}{Environment.NewLine}    Usage: {ctx.Usage}");
        }
        else
        {
            if (HelpContexts.All(c => c.Command != command))
                throw new ArgumentException($"Command \"{command}\" not found in database.");

            var ctx = HelpContexts.First(c => c.Command == command);
            Console.WriteLine($"{ctx.Command}: {ctx.Description}{Environment.NewLine}    Usage: {ctx.Usage}");
        }
    }

    internal static void Echo(string msg)
    {
        Console.WriteLine(msg); // it's as simple as that :)
    }

    internal static void Cd(string newDir)
    {
        Directory.SetCurrentDirectory(newDir);
    }

    internal static void Print(string fmt, string[] args)
    {
        fmt = Regex.Unescape(fmt);

        var fmtState = FormatState.Default;
        
        for (int i = 0, j = 0; i < fmt.Length; i++)
        {
            var state = fmt[i] switch
            {
                '%' => PrintState.Format,
                _ => PrintState.Default
            };

            if (state == PrintState.Default)
            {
                Console.Write(fmt[i]);
            }
            else
            {
                switch (fmt[i + 1])
                {
                    case 'h' when fmt[++i + 1] == 'h':
                        fmtState = FormatState.VeryShort;
                        ++i;
                        break;
                    case 'h':
                        fmtState = FormatState.Short;
                        break;
                    case 'l' when fmt[++i + 1] == 'l':
                        fmtState = FormatState.VeryLong;
                        ++i;
                        break;
                    case 'l':
                        fmtState = FormatState.Long;
                        break;
                }

                switch (fmt[++i])
                {
                    case 's':
                        Console.Write(args[j++]);
                        break;

                    case 'c':
                        Console.Write(char.Parse(args[j++]));
                        break;

                    case 'd':
                    case 'i':
                        switch (fmtState)
                        {
                            case FormatState.VeryShort:
                                Console.Write(sbyte.Parse(args[j++]));
                                break;

                            case FormatState.Short:
                                Console.Write(short.Parse(args[j++]));
                                break;

                            case FormatState.Long:
                            case FormatState.VeryLong:
                                Console.Write(long.Parse(args[j++]));
                                break;

                            case FormatState.Default:
                                Console.Write(int.Parse(args[j++]));
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(fmt));
                        }
                        break;

                    case 'u':
                        switch (fmtState)
                        {
                            case FormatState.VeryShort:
                                Console.Write(byte.Parse(args[j++]));
                                break;

                            case FormatState.Short:
                                Console.Write(ushort.Parse(args[j++]));
                                break;

                            case FormatState.Long:
                            case FormatState.VeryLong:
                                Console.Write(ulong.Parse(args[j++]));
                                break;

                            case FormatState.Default:
                                Console.Write(uint.Parse(args[j++]));
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(fmt));
                        }
                        break;

                    case 'X':
                    case 'x':
                        switch (fmtState)
                        {
                            case FormatState.VeryShort:
                                Console.Write(sbyte.Parse(args[j++]).ToString(fmt[i].ToString()));
                                break;

                            case FormatState.Short:
                                Console.Write(short.Parse(args[j++]).ToString(fmt[i].ToString()));
                                break;

                            case FormatState.Long:
                            case FormatState.VeryLong:
                                Console.Write(long.Parse(args[j++]).ToString(fmt[i].ToString()));
                                break;

                            case FormatState.Default:
                                Console.Write(int.Parse(args[j++]).ToString(fmt[i].ToString()));
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(fmt));
                        }
                        break;

                    case 'f':
                        Console.Write(float.Parse(args[j++]));
                        break;

                    default:
                        Console.Write($"${fmt[i]}");
                        break;
                }
            }
        }
        
        Console.WriteLine();
    }
}
