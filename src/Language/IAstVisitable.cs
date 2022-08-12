namespace OpenProject.ApplicationShell.Language;

/// <summary>
///     Represents an abstract syntax tree that can be visited.
/// </summary>
public interface IAstVisitable
{
    /// <summary>
    ///     Accepts the specified visitor.
    /// </summary>
    /// <param name="visitor">The visitor to accept.</param>
    /// <typeparam name="T">The type to return.</typeparam>
    /// <returns>Any <typeparamref name="T" /> to be returned.</returns>
    public T Accept<T>(IAstVisitor<T> visitor);
}