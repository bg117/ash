namespace OpenProject.ApplicationShell.Language;

/// <summary>
///     The base class of all Abstract Syntax Trees used for parsing shell syntax.
/// </summary>
public abstract class Ast : IAstVisitable
{
    /// <inheritdoc />
    public abstract T Accept<T>(IAstVisitor<T> visitor);
}

/// <summary>
///     The base class of all Abstract Syntax Trees classified as expressions.
/// </summary>
public abstract class ExpressionAst : Ast
{
}

/// <summary>
///     The base class of all Abstract Syntax Trees classified as statements.
/// </summary>
public abstract class StatementAst : Ast
{
}

/// <summary>
///     An abstract syntax tree that represents a binary operation.
/// </summary>
public class BinaryAst : ExpressionAst
{
    /// <summary>
    ///     Constructs a binary expression AST.
    /// </summary>
    /// <param name="left">The left-hand side of the expression.</param>
    /// <param name="right">The right-hand side of the expression.</param>
    /// <param name="op">The operator used in the expression.</param>
    public BinaryAst(ExpressionAst left, ExpressionAst right, Token op)
    {
        Left = left;
        Right = right;
        Operator = op;
    }

    /// <summary>
    ///     The left-hand side of the expression.
    /// </summary>
    public ExpressionAst Left { get; set; }

    /// <summary>
    ///     The right-hand side of the expression.
    /// </summary>
    public ExpressionAst Right { get; set; }

    /// <summary>
    ///     The operator used in the expression.
    /// </summary>
    public Token Operator { get; set; }

    /// <inheritdoc cref="Ast.Accept{T}" />
    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
///     An AST that represents a command (e.g. <code>ls -la</code>).
/// </summary>
public class CommandAst : ExpressionAst
{
    /// <summary>
    ///     Constructs a command AST.
    /// </summary>
    /// <param name="execute">The executable file to be executed.</param>
    /// <param name="arguments">The arguments to be passed to the executable.</param>
    public CommandAst(Token execute, Token[] arguments)
    {
        Execute = execute;
        Arguments = arguments;
    }

    /// <summary>
    ///     The executable file to be executed.
    /// </summary>
    public Token Execute { get; set; }

    /// <summary>
    ///     The arguments to be passed to the executable.
    /// </summary>
    public Token[] Arguments { get; set; }

    /// <inheritdoc />
    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
///     An AST that represents a variable assignment.
/// </summary>
public class AssignmentAst : StatementAst
{
    /// <summary>
    ///     Constructs an assignment AST.
    /// </summary>
    /// <param name="variable"></param>
    /// <param name="newValue"></param>
    public AssignmentAst(Token variable, Token newValue)
    {
        Variable = variable;
        NewValue = newValue;
    }

    /// <summary>
    ///     The name of the unit to be modified.
    /// </summary>
    public Token Variable { get; set; }

    /// <summary>
    ///     The updated value to be assigned to the variable.
    /// </summary>
    public Token NewValue { get; set; }

    /// <inheritdoc />
    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
///     An AST that represents a whole program.
/// </summary>
public class ProgramAst : StatementAst
{
    /// <summary>
    ///     Constructs a program AST.
    /// </summary>
    /// <param name="statements">The statements contained in the program.</param>
    public ProgramAst(StatementAst[] statements)
    {
        Statements = statements;
    }

    /// <summary>
    ///     The statements contained in the program.
    /// </summary>
    public StatementAst[] Statements { get; set; }

    /// <inheritdoc />
    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}

/// <summary>
///     Represents an expression statement (an expression that is also a statement).
/// </summary>
public class ExpressionStatementAst : StatementAst
{
    /// <summary>
    ///     Constructs an expression statement AST.
    /// </summary>
    /// <param name="expression">The expression to be contained in the statement.</param>
    public ExpressionStatementAst(ExpressionAst expression)
    {
        Expression = expression;
    }

    /// <summary>
    ///     The expression to be contained in the statement
    /// </summary>
    public ExpressionAst Expression { get; set; }

    /// <inheritdoc />
    public override T Accept<T>(IAstVisitor<T> visitor)
    {
        return visitor.Visit(this);
    }
}