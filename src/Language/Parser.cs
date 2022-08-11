namespace OpenProject.ApplicationShell.Language;

public class Parser
{
    private readonly List<Token> _tokens = new();

    private int _position;

    public Parser(Lexer lexer)
    {
        var token = lexer.NextToken();
        while (token.Type != TokenType.Eof)
        {
            _tokens.Add(token);
            token = lexer.NextToken();
        }
    }

    private Token Current => !IsEof() ? _tokens[_position] : new Token { Type = TokenType.Eof };

    private bool IsEof()
    {
        return _position >= _tokens.Count;
    }

    private void Consume(params TokenType[] types)
    {
        if (types.All(type => Current.Type != type))
            throw new InvalidOperationException(
                $"Expected {string.Join(", ", types.Select(type => type.ToString()))}, but got {Current.Type} instead at {Current.Line}:{Current.Column}");

        ++_position;
    }

    public Ast Parse()
    {
        return ParseProgram();
    }

    private Ast ParseProgram()
    {
        var statements = new List<StatementAst>();
        while (!IsEof())
            statements.Add(ParseStatement());

        return new ProgramAst
        (statements.ToArray()
        );
    }

    private StatementAst ParseStatement()
    {
        if (_position + 1 < _tokens.Count && _tokens[_position + 1].Type == TokenType.Equals)
            return ParseAssignmentStatement();

        var expr = ParseExpression();
        if (Current.Type == TokenType.Semicolon)
            Consume(TokenType.Semicolon);

        return new ExpressionStatementAst
        (
            expr
        );
    }

    private StatementAst ParseAssignmentStatement()
    {
        var assignee = Current;

        Consume(TokenType.Unit);
        Consume(TokenType.Equals);

        var value = Current;

        ++_position;

        if (Current.Type == TokenType.Semicolon)
            Consume(TokenType.Semicolon);

        return new AssignmentAst
        (
            assignee,
            value
        );
    }

    private ExpressionAst ParseExpression()
    {
        return ParseOrExpression();
    }

    private ExpressionAst ParseOrExpression()
    {
        var lhs = ParseAndExpression();
        while (!IsEof() && Current.Type == TokenType.Or)
        {
            var token = Current;

            Consume(TokenType.Or);
            lhs = new BinaryAst
            (
                lhs,
                ParseAndExpression(),
                token
            );
        }

        return lhs;
    }

    private ExpressionAst ParseAndExpression()
    {
        var lhs = ParsePipeExpression();
        while (!IsEof() && Current.Type == TokenType.And)
        {
            var token = Current;

            Consume(TokenType.And);
            lhs = new BinaryAst
            (
                lhs,
                ParsePipeExpression(),
                token
            );
        }

        return lhs;
    }

    private ExpressionAst ParsePipeExpression()
    {
        var lhs = ParsePrimaryExpression();
        while (!IsEof() && Current.Type == TokenType.Pipe)
        {
            var token = Current;

            Consume(TokenType.Pipe);
            lhs = new BinaryAst
            (
                lhs,
                ParsePrimaryExpression(),
                token
            );
        }

        return lhs;
    }

    private ExpressionAst ParsePrimaryExpression()
    {
        switch (Current.Type)
        {
            case TokenType.LeftParen:
            {
                Consume(TokenType.LeftParen);
                var expr = ParseExpression();
                Consume(TokenType.RightParen);

                return expr;
            }
            case TokenType.Unit:
            case TokenType.Variable:
                return ParseCommandExpression();
            default:
                throw new InvalidOperationException($"Unexpected {Current.Type} at {Current.Line}:{Current.Column}");
        }
    }

    private ExpressionAst ParseCommandExpression()
    {
        if (Current.Type is not (TokenType.Unit or TokenType.Variable))
            throw new InvalidOperationException($"Unexpected {Current.Type} at {Current.Line}:{Current.Column}");

        var exec = Current;
        var args = new List<Token>();

        ++_position;

        while (!IsEof() && Current.Type is TokenType.Unit or TokenType.Variable)
        {
            args.Add(Current);
            ++_position;
        }

        return new CommandAst
        (
            exec,
            args.ToArray()
        );
    }
}