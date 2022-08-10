using System.Text.RegularExpressions;

namespace OpenProject.ASH;

/// <summary>
///     Built-in commands helper class. Exceptions are to be handled by the calling code.
/// </summary>
public static class BuiltInCommands {
    private static readonly HelpContext[] HelpContexts =
    {
        new()
        {
            Command = "help",
            Description =
                "Shows the name and description of every command.",
            Usage = "help [command]"
        },
        new()
        {
            Command = "echo",
            Description =
                "Prints the succeeding arguments to the console and a new line. Quotes won't be removed. They will be printed as-is.",
            Usage = "echo [text...]"
        },
        new()
        {
            Command = "print",
            Description =
                "Formats and prints the text according to the format string. Quotes will be removed. Also un-escapes escape sequences (like \\n, \\r, etc.).",
            Usage = "print <format> [arg1, [arg2, [...]]]"
        },
        new()
        {
            Command     = "cd",
            Description = "Changes the current directory.",
            Usage       = "cd <directory>"
        },
        new()
        {
            Command = "ls",
            Description =
                "Lists the files in the current directory, or optionally, in the directory specified.",
            Usage = "ls [-a] [-l] [-h] [directory]"
        },
        new()
        {
            Command     = "exit",
            Description = "Exits the shell.",
            Usage       = "exit [code]"
        }
    };

    /// <summary>
    ///     Display help contained in <see cref="HelpContexts" />.
    /// </summary>
    /// <param name="command">(optional) command to describe. If not specified, displays help for all commands.</param>
    /// <exception cref="ArgumentException">
    ///     If <paramref name="command" /> does not exist in <see cref="HelpContexts" />,
    ///     throws an <see cref="ArgumentException" />.
    /// </exception>
    public static void Help(string? command = null)
    {
        if (command == null) {
            foreach (var ctx in HelpContexts) {
                Console
                   .WriteLine($"{ctx.Command}: {ctx.Description}{Environment.NewLine}    Usage: {ctx.Usage}");
                Console.WriteLine();
            }
        }
        else {
            if (HelpContexts.All(c => c.Command != command)) {
                throw new
                    ArgumentException($"Command \"{command}\" not found in database.");
            }

            var ctx = HelpContexts.First(c => c.Command == command);
            Console.WriteLine($"{ctx.Command}: {ctx.Description}{Environment.NewLine}    Usage: {ctx.Usage}");
        }
    }

    /// <summary>
    ///     Prints the string to the console with a new line.
    /// </summary>
    /// <param name="msg">String to print to the console.</param>
    public static void Echo(string msg)
    {
        Console.WriteLine(msg); // it's as simple as that :)
    }

    /// <summary>
    ///     Changes directory to the path specified.
    /// </summary>
    /// <param name="newDir">Directory to change to.</param>
    public static void Cd(string newDir)
    {
        Directory.SetCurrentDirectory(newDir);
    }

    /// <summary>
    ///     Lists the contents of the directory specified.
    ///     If <paramref name="listFormat" />
    ///     is specified, lists contents in list format. See README for more information.
    ///     If <paramref name="all" /> is specified, also displays hidden and system files.
    ///     If <paramref name="humanReadable" /> is specified, displays file sizes in human-readable
    ///     format.
    /// </summary>
    /// <param name="directory">Directory to list files.</param>
    /// <param name="listFormat">Tells whether to display in list format or not.</param>
    /// <param name="all">Tells whether to display hidden and system files.</param>
    /// <param name="humanReadable">Tells whether to print sizes in human-readable format.</param>
    public static void Ls(string directory,
                          bool   listFormat,
                          bool   all,
                          bool   humanReadable)
    {
        var dir   = new DirectoryInfo(directory);
        var list  = dir.GetFileSystemInfos();
        var files = dir.GetFiles();

        var nameLen = list.Max(x => x.Name.Length)               + 2;
        var sizeLen = files.Max(x => x.Length.ToString().Length) + 2;

        if (listFormat) {
            var header =
                $"{"Mode",-7} | {"Name".PadRight(nameLen)} | {"Date Modified",-20 /* 1970/01/01 00:00:00 */} | {"Date Created",-20} | {"Size".PadRight(sizeLen)}";

            Console.WriteLine(header);
            Console.WriteLine(new string('-', header.Length));

            var printFileInfo = new Action<FileSystemInfo>(info =>
                                                           {
                                                               var attribute =
                                                                   string.Empty;

                                                               // darhsl attributes
                                                               attribute +=
                                                                   info
                                                                      .Attributes
                                                                      .HasFlag(FileAttributes
                                                                                  .Directory)
                                                                       ? "d"
                                                                       : "-";
                                                               attribute +=
                                                                   info
                                                                      .Attributes
                                                                      .HasFlag(FileAttributes
                                                                                  .Archive)
                                                                       ? "a"
                                                                       : "-";
                                                               attribute +=
                                                                   info
                                                                      .Attributes
                                                                      .HasFlag(FileAttributes
                                                                                  .ReadOnly)
                                                                       ? "r"
                                                                       : "-";
                                                               attribute +=
                                                                   info
                                                                      .Attributes
                                                                      .HasFlag(FileAttributes
                                                                                  .Hidden)
                                                                       ? "h"
                                                                       : "-";
                                                               attribute +=
                                                                   info
                                                                      .Attributes
                                                                      .HasFlag(FileAttributes
                                                                                  .System)
                                                                       ? "s"
                                                                       : "-";
                                                               attribute +=
                                                                   info
                                                                      .Attributes
                                                                      .HasFlag(FileAttributes
                                                                                  .ReparsePoint)
                                                                       ? "l"
                                                                       : "-";

                                                               // attributes | name | last write time | creation time | file size (if -h, sort by B/KB/MB/GB/TB/PB/EB)
                                                               Console
                                                                  .WriteLine(
                                                                             $"{attribute}  | " +
                                                                             $"{info.Name.PadRight(nameLen)} | " +
                                                                             $"{info.LastWriteTime:yyyy/MM/dd HH:mm:ss}  | " +
                                                                             $"{info.CreationTime:yyyy/MM/dd HH:mm:ss}  | " +
                                                                             $"{(info is FileInfo file ? humanReadable ? StringHelpers.BytesToString(file.Length) : file.Length : "")}"
                                                                            );
                                                           });

            foreach (var info in list) {
                if (info.Attributes.HasFlag(FileAttributes.Hidden)) {
                    if (all)
                        printFileInfo(info);
                    else
                        continue;
                }

                printFileInfo(info);
            }
        }
        else // ignore human-readable flag
        {
            // determine the maximum number of text with length nameLen can fit inside the console
            var newlineEveryX = Console.BufferWidth / nameLen;
            var i             = 0;

            foreach (var info in list) {
                if (i++ >= newlineEveryX) {
                    Console.WriteLine();
                    i = 0;
                }

                if (info.Attributes.HasFlag(FileAttributes.Hidden)) {
                    if (all)
                        Console.Write($"{info.Name.PadRight(nameLen)}");
                    else
                        continue;
                }

                Console.Write(info.Name.PadRight(nameLen));
            }
        }
    }

    /// <summary>
    ///     Prints the format string to the console, equivalent to C <code>printf</code>.
    /// </summary>
    /// <param name="fmt">Format string to work on.</param>
    /// <param name="args">Arguments to replace the placeholders in the format string with.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="args" /> fall short of the amount of format specifiers
    ///     in the format string, an <see cref="ArgumentOutOfRangeException" /> gets thrown.
    /// </exception>
    public static void Print(string fmt, string[] args)
    {
        fmt = Regex.Unescape(fmt);

        var fmtState = FormatState.Default;

        for (int i = 0, j = 0; i < fmt.Length; i++) {
            var state = fmt[i] switch
                        {
                            '%' => PrintState.Format,
                            _   => PrintState.Default
                        };

            if (state == PrintState.Default)
                Console.Write(fmt[i]);
            else {
                switch (fmt[i + 1]) {
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

                switch (fmt[++i]) {
                    case 's':
                        Console.Write(args[j++]);
                        break;

                    case 'c':
                        Console.Write(char.Parse(args[j++]));
                        break;

                    case 'd':
                    case 'i':
                        switch (fmtState) {
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
                                throw new
                                    ArgumentOutOfRangeException(nameof(fmt));
                        }

                        break;

                    case 'u':
                        switch (fmtState) {
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
                                throw new
                                    ArgumentOutOfRangeException(nameof(fmt));
                        }

                        break;

                    case 'X':
                    case 'x':
                        switch (fmtState) {
                            case FormatState.VeryShort:
                                Console.Write(sbyte.Parse(args[j++])
                                                   .ToString(fmt[i]
                                                                .ToString()));
                                break;

                            case FormatState.Short:
                                Console.Write(short.Parse(args[j++])
                                                   .ToString(fmt[i]
                                                                .ToString()));
                                break;

                            case FormatState.Long:
                            case FormatState.VeryLong:
                                Console.Write(long.Parse(args[j++])
                                                  .ToString(fmt[i].ToString()));
                                break;

                            case FormatState.Default:
                                Console.Write(int.Parse(args[j++])
                                                 .ToString(fmt[i].ToString()));
                                break;

                            default:
                                throw new
                                    ArgumentOutOfRangeException(nameof(fmt));
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

    private enum PrintState {
        Default,
        Format
    }

    private enum FormatState {
        Default,
        VeryShort,
        Short,
        Long,
        VeryLong
    }
}
