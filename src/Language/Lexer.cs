namespace OpenProject.ApplicationShell.Language;

/// <summary>
///     Breaks up the shell source into tokens that are
///     recognized by the parser.
/// </summary>
public class Lexer
{
    private readonly string _source;
    private int _line, _column;
    private int _position;

    /// <summary>
    ///     Constructs a lexer.
    /// </summary>
    /// <param name="source">The shell source to analyze.</param>
    public Lexer(string source)
    {
        // initialize to default values
        _source = source;
        _line = 1;
        _column = 1;
        _position = 0;
    }

    private char Current => _source[_position];

    /// <summary>
    ///     Checks if the string contains illegal characters.
    /// </summary>
    /// <param name="s">The string to check.</param>
    /// <returns>
    ///     If the string contains illegal characters, true is returned.
    ///     Otherwise, false.
    /// </returns>
    public static bool StringContainsIllegalCharacters(string s)
    {
        return s.Any(CharIsIllegalCharacter);
    }

    /// <summary>
    ///     Checks if the character is an illegal character.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>
    ///     If the character is an illegal character, true is returned.
    ///     Otherwise, false.
    /// </returns>
    public static bool CharIsIllegalCharacter(char c)
    {
        return c is '\n' or ' ' or '=' or '|' or '&' or ';' or '=' or '(' or ')';
    }

    /// <summary>
    ///     Returns the next token from the stream.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Token NextToken()
    {
        var token = new Token
        {
            Value = "",
            Type = TokenType.Eof,
            Line = _line,
            Column = _column
        };

        SkipWhitespace();
        SkipComment();

        if (IsEof())
            return token;

        var nonDynamic = true;

        switch (Current)
        {
            case '=':
                token.Type = TokenType.Equals;
                break;
            case ';':
                token.Type = TokenType.Semicolon;
                break;
            case '(':
                token.Type = TokenType.LeftParen;
                break;
            case ')':
                token.Type = TokenType.RightParen;
                break;
            default:
                nonDynamic = false;
                break;
        }

        token.Value = Current.ToString();

        if (!nonDynamic)
            switch (Current)
            {
                case '&' when _position + 1 < _source.Length && _source[_position + 1] == '&':
                    token.Value = "&&";
                    token.Type = TokenType.And;

                    Advance();
                    break;
                case '&':
                    throw new InvalidOperationException($"Invalid operator '&' at {_line}:{_column}");
                case '|' when _position + 1 < _source.Length && _source[_position + 1] == '|':
                    token.Value = "||";
                    token.Type = TokenType.Or;

                    Advance();
                    break;
                case '|':
                    token.Value = "|";
                    token.Type = TokenType.Pipe;
                    break;
                case '$':
                    Advance();

                    var variable = CollectUnit();
                    variable.Value = '$' + variable.Value;
                    variable.Type = TokenType.Variable;

                    return variable;
                default:
                    return CollectUnit();
            }

        Advance();

        return token;
    }

    private bool IsEof()
    {
        return _position >= _source.Length;
    }

    private void SkipWhitespace()
    {
        if (IsEof())
            return;

        while (!IsEof() && char.IsWhiteSpace(Current))
            Advance();
    }

    private void SkipComment()
    {
        if (IsEof())
            return;

        if (Current != '#')
            return;

        Advance();

        while (!IsEof() && Current != '\n')
            Advance();
    }

    private void Advance()
    {
        ++_column;
        if (Current == '\n')
        {
            ++_line;
            _column = 0;
        }

        ++_position;
    }

    private Token CollectUnit()
    {
        var token = new Token
        {
            Value = "",
            Type = TokenType.Eof,
            Line = _line,
            Column = _column
        };

        if (IsEof())
            return token;

        token.Type = TokenType.Unit;

        while (!IsEof() && !CharIsIllegalCharacter(Current))
            if (Current == '\"')
            {
                Advance();

                while (!IsEof() && Current != '\"')
                {
                    token.Value += HandleEscapeSequences();
                    Advance();
                }

                if (IsEof())
                    throw new InvalidOperationException($"Unterminated quoted unit at {_line}:{_column}");

                Advance();
            }
            else
            {
                token.Value += HandleEscapeSequences();
                Advance();
            }

        return token;
    }

    private string HandleEscapeSequences()
    {
        var add = Current.ToString();
        if (Current != '\\') return add;

        Advance();
        if (IsEof())
            throw new InvalidOperationException($"Singular backslash at {_line}:{_column}");

        add = Current switch
        {
            'n' => "\n",
            'r' => "\r",
            't' => "\t",
            '\\' => "\\",
            '\"' => "\"",
            _ => "\\" + Current
        };

        return add;
    }
}