using System.Diagnostics;
using System.Text.RegularExpressions;
using OpenProject.ApplicationShell.Helpers;

namespace OpenProject.ApplicationShell.Language;

/// <summary>
///     A shell evaluator.
/// </summary>
public class Evaluator : IAstVisitor<int>
{
    /// <summary>
    ///     The root node of the abstract syntax tree.
    /// </summary>
    public Ast? Root { get; set; }

    /// <summary>
    ///     The environment of the shell.
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    private static readonly Regex VarPattern = new(@"(?<!\\)\$(\w+)"); // capture if not preceded by backslash

    private string _pipeToStdin = string.Empty; // the thing to be piped to stdin of the next process

    // flags that indicate whether to read/write to stdout/stdin, respectively
    private bool _readStdout = false;
    private bool _writeStdin = false;

    /// <summary>
    ///     Evaluates the AST starting from the root.
    /// </summary>
    /// <returns>The exit code of the last command executed.</returns>
    public int Evaluate()
    {
        return Root?.Accept(this) ?? 0;
    }

    public int Visit(Ast node)
    {
        return node.Accept(this);
    }

    public int Visit(ExpressionAst node)
    {
        return node.Accept(this);
    }

    public int Visit(StatementAst node)
    {
        return node.Accept(this);
    }

    public int Visit(CommandAst node)
    {
        var strArgs = node.Arguments.Select(s => s.Value).ToList();

        // replace $var stuff
        var exec = ReplaceVars(node.Execute.Value);
        var args = ReplaceVars(string.Join(' ', strArgs.Select(s => '"' + s + '"')));

        // check if one of built-in commands
        var builtin = HandleBuiltIns(exec, strArgs.ToArray(), out var ret);
        if (builtin)
            return ret;

        // create new process to be executed
        var proc = new Process();
        proc.StartInfo.FileName = exec;
        proc.StartInfo.Arguments = args;
        proc.StartInfo.UseShellExecute = false;

        // if _readStdout/_writeStdin is set then allow redirection
        // to other streams
        if (_readStdout)
            proc.StartInfo.RedirectStandardOutput = true;
        if (_writeStdin)
            proc.StartInfo.RedirectStandardInput = true;

        proc.Start();

        if (_readStdout)
            _pipeToStdin = proc.StandardOutput.ReadToEnd(); // capture text from stdout and store it in
        // _pipeToStdin for the next process

        if (_writeStdin)
        {
            proc.StandardInput.Write(_pipeToStdin); // pipe _pipeToStdin to stdin
            proc.StandardInput.Close(); // close stdin to indicate that piping is done
        }

        proc.WaitForExit();

        return proc.ExitCode;
    }

    private static bool HandleBuiltIns(string exec, string[] strArgs, out int exit)
    {
        switch (exec)
        {
            case "help":
                switch (strArgs.Length)
                {
                    case > 1:
                        ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Too many arguments for help",
                            ConsoleColor.Red);
                        exit = 1;
                        return true;
                    case 1:
                        BuiltInCommands.Help(strArgs[0]);
                        break;
                    default:
                        BuiltInCommands.Help();
                        break;
                }

                break;
            
            case "chdir":
                switch (strArgs.Length)
                {
                    case > 1:
                        ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Too many arguments for chdir", ConsoleColor.Red);
                        exit = 1;
                        return true;
                    case 0:
                        ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Too few arguments for chdir", ConsoleColor.Red);
                        exit = 2;
                        return true;
                }

                BuiltInCommands.Chdir(strArgs[0]);
                break;
            
            case "strfmt":
                if (strArgs.Length == 0)
                {
                    ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Too little arguments for strfmt",
                        ConsoleColor.Red);
                    exit = 1;
                    return true;
                }
                
                BuiltInCommands.Strfmt(strArgs[0], strArgs[1..]);
                break;
            
            case "echo":
                BuiltInCommands.Echo(string.Join(" ", strArgs));
                break;

            case "list":
            {
                if (strArgs.Length > 4)
                {
                    ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Too many arguments for list",
                        ConsoleColor.Red);
                    exit = 1;
                    return true;
                }
                
                bool a, h;
                var l = a = h = false;

                bool Pred(string s) => s is not ("-lah" or "-alh" or "-ahl" or "-lha" or "-lh" or "-hl" or "-ah"
                    or "-ha" or "-la" or "-al" or "-l" or "-a" or "-h");

                if (strArgs.Contains("-lah") || strArgs.Contains("-alh") || strArgs.Contains("-ahl") ||
                    strArgs.Contains("-lha"))
                {
                    l = a = h = true;
                }
                else if (strArgs.Contains("-lh") || strArgs.Contains("-hl"))
                {
                    l = h = true;
                }
                else if (strArgs.Contains("-ah") || strArgs.Contains("-ha"))
                {
                    a = h = true;
                }
                else if (strArgs.Contains("-la") || strArgs.Contains("-al"))
                {
                    l = a = true;
                }
                else if (strArgs.Contains("-l"))
                {
                    l = true;
                }
                else if (strArgs.Contains("-a"))
                {
                    a = true;
                }
                else if (strArgs.Contains("-h"))
                {
                    h = true;
                }
                else if (strArgs.Contains("-l") && strArgs.Contains("-a") && strArgs.Contains("-h"))
                {
                    l = a = h = true;
                }

                var dir = Directory.GetCurrentDirectory();
                if (strArgs.Any(Pred))
                    dir = strArgs.First(Pred);
                
                BuiltInCommands.List(dir, l, a, h);
            }
                break;
            
            case "exit":
            {
                if (strArgs.Length > 1)
                {
                    ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Too many arguments for exit",
                        ConsoleColor.Red);
                    exit = 1;
                    return true;
                }

                var code = 0;
                if (strArgs.Length == 1 && !int.TryParse(strArgs[0], out code))
                {
                    ConsoleHelpers.ColoredWriteLine(Console.Error, "[Error]: Argument for exit must be an integer",
                        ConsoleColor.Red);
                    exit = 2;
                    return true;
                }

                System.Environment.Exit(strArgs.Length == 0 ? 0 : code);
            }
                break;
                
            default:
                exit = 0;
                return false;
        }

        exit = 0;
        return true;
    }

    private string ReplaceVars(string s)
    {
        return VarPattern.Replace(s, match =>
        {
            var key = match.Groups[1].Value;
            return Environment.ContainsKey(key)
                ? Environment[key]
                : '$' + key;
        });
    }

    public int Visit(BinaryAst node)
    {
        // reset pipe flags
        _readStdout = false;
        _writeStdin = false;

        switch (node.Operator.Type)
        {
            // if AND, then return if first command failed
            // otherwise, continue evaluating
            case TokenType.And:
            {
                var lhs = node.Left.Accept(this);
                return lhs == 0 ? node.Right.Accept(this) : lhs;
            }
            // if OR, return success if first command succeeded
            // otherwise, continue evaluating
            case TokenType.Or:
            {
                var lhs = node.Left.Accept(this);
                return lhs != 0 ? node.Right.Accept(this) : lhs;
            }
            // pipe _pipeToStdin to the next process
            case TokenType.Pipe:
            {
                var oldOut = Console.Out; // save old stdout
                var sw = new StringWriter();

                Console.SetOut(sw); // this is for built-in commands
                // external programs are handled using Process.StartInfo.RedirectStandardInput/Output

                _readStdout = true; // for the first process, read the stdout and do not write to stdin
                _writeStdin = false;
                node.Left.Accept(this);

                Console.SetOut(oldOut);

                _readStdout = false; // for the second bit, write to stdin (pipe) but do not read from stdout
                _writeStdin = true;
                var rhs = node.Right.Accept(this);

                _pipeToStdin = string.Empty; // reset pipe buffer

                return rhs; // return exit code of second command
            }
            default: // invalid operator
                throw new InvalidOperationException(
                    $"Invalid operator \"{node.Operator.Value}\" at {node.Operator.Line}:{node.Operator.Column}");
        }
    }

    public int Visit(AssignmentAst node)
    {
        // check if trying to assign to variable that contains illegal characters
        // this is a flaw in the lexer, it recognizes quoted stuff and non-quoted stuff as one type
        if (Lexer.StringContainsIllegalCharacters(node.Variable.Value))
            throw new InvalidOperationException(
                $"Trying to assign to a unit that contains illegal characters at {node.Variable.Line}:{node.Variable.Column}");

        var value = ReplaceVars(node.NewValue.Value);

        // set corresponding key in the env
        if (!Environment.ContainsKey(node.Variable.Value))
            Environment.Add(node.Variable.Value, value);
        else
            Environment[node.Variable.Value] = value;

        return 0;
    }

    public int Visit(ProgramAst node)
    {
        var exit = 0;
        foreach (var statement in node.Statements)
            exit = statement.Accept(this);

        return exit;
    }

    public int Visit(ExpressionStatementAst node)
    {
        return node.Expression.Accept(this);
    }
}