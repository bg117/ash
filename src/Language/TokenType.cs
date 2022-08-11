namespace OpenProject.ApplicationShell.Language;

public enum TokenType
{
    Eof,

    Unit, // anything except for objects that start with $, including those surrounded by quotation marks
    Variable, // $var

    Equals,
    And,
    Or,
    Pipe,

    LeftParen,
    RightParen,

    Semicolon
}