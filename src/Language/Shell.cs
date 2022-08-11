namespace OpenProject.ApplicationShell.Language;

public class Shell
{
    /// <summary>
    ///     List of built-in variables. The order of these won't change, ever.
    /// </summary>
    public static readonly string[] BuiltInVariables =
    {
        "prompt_fmt",
        "prompt_fmt_u_color",
        "prompt_fmt_m_color",
        "prompt_fmt_c_color",
        "prompt_fmt_e_success_color",
        "prompt_fmt_e_fail_color",
        "quiet_startup",
        "path",
        "home"
    };

    /// <summary>
    ///     Exit code of the last command executed.
    /// </summary>
    public int LastExitCode { get; set; }

    /// <summary>
    ///     The environment of the shell.
    /// </summary>
    public Dictionary<string, string> Environment { get; set; } = new();

    private readonly Evaluator _evaluator = new();

    public int Execute(string input)
    {
        var lexer = new Lexer(input);
        var parser = new Parser(lexer);
        var program = parser.Parse();
        _evaluator.Root = program;
        _evaluator.Environment = Environment;

        LastExitCode = _evaluator.Evaluate();
        return LastExitCode;
    }

    public int ExecuteFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Cannot find file \"{path}\"");

        var lines = File.ReadAllLines(path);
        var exit = 0;

        foreach (var line in lines)
            exit = Execute(line); // run this on each line of the config

        return exit;
    }
}