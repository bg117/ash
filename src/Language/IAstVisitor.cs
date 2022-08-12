namespace OpenProject.ApplicationShell.Language;

/// <summary>
///     Represents a visitor of an abstract syntax tree.
/// </summary>
/// <typeparam name="T">Any <typeparamref name="T" /> to be returned.</typeparam>
public interface IAstVisitor<out T>
{
    public T Visit(Ast node);
    public T Visit(ExpressionAst node);
    public T Visit(StatementAst node);
    public T Visit(CommandAst node);
    public T Visit(BinaryAst node);
    public T Visit(AssignmentAst node);
    public T Visit(ProgramAst node);
    public T Visit(ExpressionStatementAst node);
}