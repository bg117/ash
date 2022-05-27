namespace OpenProject.ASH;

/// <summary>
///     Used for help printing.
/// </summary>
public class HelpContext {
    /// <summary>
    ///     Command.
    /// </summary>
    public string Command     { get; init; } = string.Empty;

    /// <summary>
    ///     Description of the command.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    ///     The usage of the command.
    /// </summary>
    public string Usage       { get; init; } = string.Empty;
}
