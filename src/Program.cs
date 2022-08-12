using System.Reflection;
using OpenProject.ApplicationShell.Helpers;
using OpenProject.ApplicationShell.Language;

namespace OpenProject.ApplicationShell;

internal static class Program
{
    private const string DefaultFormat = @"%u@%m:%c$ ";
    private const string ConfigFile = "ash.cfg";

    private static readonly string ProgramVersion =
        (Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0))
        .ToString(3);

    private static readonly CommandLineOption[] Options =
    {
        new()
        {
            Option = "quiet",
            IsLongOption = true,
            RequiresArgument = false,
            Description =
                "Runs the program quietly."
        },
        new()
        {
            Option = "q",
            IsLongOption = false,
            RequiresArgument = false,
            Description = "See --quiet."
        },
        new()
        {
            Option = "execute",
            IsLongOption = true,
            RequiresArgument = true,
            Description =
                $"Executes a command before parsing {ConfigFile}. Optionally pass -c if the shell must exit when a command fails."
        },
        new()
        {
            Option = "x",
            IsLongOption = false,
            RequiresArgument = true,
            Description = "See --execute."
        },
        new()
        {
            Option = "version",
            IsLongOption = true,
            RequiresArgument = false,
            Description = "Shows the version and exits the program."
        },
        new()
        {
            Option = "v",
            IsLongOption = false,
            RequiresArgument = false,
            Description = "See --version."
        },
        new()
        {
            Option = "help",
            IsLongOption = true,
            RequiresArgument = false,
            Description = "Shows this help message and exits the program."
        },
        new()
        {
            Option = "h",
            IsLongOption = false,
            RequiresArgument = false,
            Description = "See --help."
        }
    };

    private static int Main(string[] args)
    {
        Console.CancelKeyPress += (_, e) => e.Cancel = true;

        var execCmds = new List<string>();
        var quiet = false;
        var help = false;
        var version = false;
        var execute = false;

        CommandLineHelpers.ParseCommandLine(Options, args, (opt, arg) =>
        {
            switch (opt)
            {
                case "h" or "help":
                    help = true;
                    break;
                case "v" or "version":
                    version = true;
                    break;
                case "q" or "quiet":
                    quiet = true;
                    break;
                case "x" or "execute":
                    execute = true;
                    execCmds.AddRange(arg);
                    break;
            }
        });

        var kvEx = new Dictionary<string, string>();
        var kv = new Dictionary<string, string>();

        var shell = new Shell
        {
            Environment = kvEx
        };

        var home =
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var cfg = Path.Combine(home, ConfigFile);

        // if there are -x arguments, execute them first
        if (execute)
            foreach (var cmd in execCmds)
                shell.Execute(cmd);

        if (help)
        {
            Console.WriteLine($"ASH version {ProgramVersion}");
            foreach (var opt in Options)
                Console.WriteLine($"{opt.Option}: {opt.Description}");
            return 0;
        }

        if (version)
        {
            Console.WriteLine($"ASH version {ProgramVersion}");
            return 0;
        }

        if (!File.Exists(cfg))
            File.AppendAllLines(
                cfg,
                new[]
                {
                    "#RC VERSION 001",
                    $"{Shell.BuiltInVariables[0]} = \"{DefaultFormat}\"",
                    $"{Shell.BuiltInVariables[1]} = white",
                    $"{Shell.BuiltInVariables[2]} = white",
                    $"{Shell.BuiltInVariables[3]} = white",
                    $"{Shell.BuiltInVariables[4]} = white",
                    $"{Shell.BuiltInVariables[5]} = white",
                    $"{Shell.BuiltInVariables[6]} = false",
                    $"{Shell.BuiltInVariables[7]} = \"{Environment.GetEnvironmentVariable("PATH")?.Replace(Path.PathSeparator, '|')}\"",
                    $"{Shell.BuiltInVariables[8]} = \"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\""
                });

        shell.ExecuteFile(cfg);

        // merge kvEx and kv, prioritizing kvEx values if there are duplicates
        shell.Environment = shell.Environment.Merge(kv);

        if (!shell.Environment.ContainsKey(Shell.BuiltInVariables[0]))
            shell.Environment.Add(Shell.BuiltInVariables[0], DefaultFormat);

        if (!quiet)
            if (shell.Environment.ContainsKey(Shell.BuiltInVariables[6]))
                _ = bool.TryParse(shell.Environment[Shell.BuiltInVariables[6]],
                    out quiet);

        var userColor = ConsoleColor.White;
        var machineColor = ConsoleColor.White;
        var cwdColor = ConsoleColor.White;
        var exitColorSuccess = ConsoleColor.White;
        var exitColorFail = ConsoleColor.White;

        if (!shell.Environment.ContainsKey(Shell.BuiltInVariables[1]))
            shell.Environment.Add(Shell.BuiltInVariables[1], userColor.ToString());

        if (!shell.Environment.ContainsKey(Shell.BuiltInVariables[2]))
            shell.Environment.Add(Shell.BuiltInVariables[2],
                machineColor.ToString());

        if (!shell.Environment.ContainsKey(Shell.BuiltInVariables[3]))
            shell.Environment.Add(Shell.BuiltInVariables[3], cwdColor.ToString());

        if (!shell.Environment.ContainsKey(Shell.BuiltInVariables[4]))
            shell.Environment.Add(Shell.BuiltInVariables[4],
                exitColorSuccess.ToString());

        if (!shell.Environment.ContainsKey(Shell.BuiltInVariables[5]))
            shell.Environment.Add(Shell.BuiltInVariables[5],
                exitColorFail.ToString());

        if (!quiet)
        {
            Console.WriteLine($"ASH (Application shell) version {ProgramVersion}");
            Console.WriteLine(
                $"To hide this message, run ASH with --execute/-x \"{Shell.BuiltInVariables[6]} = true\" (overrides {ConfigFile}) " +
                $"or the --quiet/-q flag (overrides both --execute and {ConfigFile}).");
            Console.WriteLine("To print a list of all built-in commands, type \"help\" then press ENTER.");
            Console.WriteLine();
        }

        while (true)
        {
            var user = Environment.UserName;
            var host = Environment.MachineName;

            var cwd = Environment.CurrentDirectory;

            _ = Enum.TryParse(shell.Environment[Shell.BuiltInVariables[1]], true,
                out userColor);
            _ = Enum.TryParse(shell.Environment[Shell.BuiltInVariables[2]], true,
                out machineColor);
            _ = Enum.TryParse(shell.Environment[Shell.BuiltInVariables[3]], true,
                out cwdColor);
            _ = Enum.TryParse(shell.Environment[Shell.BuiltInVariables[4]], true,
                out exitColorSuccess);
            _ = Enum.TryParse(shell.Environment[Shell.BuiltInVariables[5]], true,
                out exitColorFail);

            ConsoleHelpers.ColoredWrite(Console.Out,
                shell.Environment[Shell.BuiltInVariables[0]]
                    .Replace("%u",
                        $"[{user}]")
                    .Replace("%m",
                        $"[{host}]")
                    .Replace("%c",
                        $"[{cwd}]")
                    .Replace("\\n", "\n")
                    .Replace("%e",
                        $"[{shell.LastExitCode}]"),
                userColor,
                machineColor,
                cwdColor,
                shell.LastExitCode == 0
                    ? exitColorSuccess
                    : exitColorFail);

            var input = string.Empty;
            var firstIter = true;
            do
            {
                if (!firstIter)
                    Console.Write(">>> ");

                firstIter = false;

                var tmp = Console.ReadLine();
                if (tmp == null)
                    break;

                tmp = tmp.Trim();
                if (tmp.EndsWith("\\"))
                {
                    input += '\n' + tmp[..^1] /* don't include backslash */;
                }
                else
                {
                    input += ' ' + tmp;
                    break;
                }
            } while (true);

            if (string.IsNullOrWhiteSpace(input))
                continue;

            input = input.Trim();

            try
            {
                shell.Execute(input);
            }
            catch (Exception e)
            {
                ConsoleHelpers.ColoredWriteLine(Console.Error,
                    $"[Error]: {e.Message}",
                    ConsoleColor.Red);
            }
        }
    }
}