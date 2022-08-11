namespace OpenProject.ApplicationShell.Language;

public struct Token
{
    public string Value { get; set; }
    public TokenType Type { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}