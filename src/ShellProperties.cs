namespace OpenProject.ASH;

/// <summary>
///     Properties of the shell.
/// </summary>
public static class ShellProperties {
    /// <summary>
    ///     Exit code of the last command executed.
    /// </summary>
    public static int LastExitCode { get; set; }

    /// <summary>
    ///     Error message of the last failed command.
    /// </summary>
    public static string ExitErrorMessage { get; set; } = string.Empty;

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
}
