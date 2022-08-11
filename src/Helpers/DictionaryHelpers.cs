namespace OpenProject.ApplicationShell.Helpers;

/// <summary>
///     Functions that help with <see cref="Dictionary{TKey,TValue}" />.
/// </summary>
internal static class DictionaryHelpers
{
    /// <summary>
    ///     Merges <paramref name="priority" /> and <paramref name="add" />, prioritizing values
    ///     contained in <paramref name="priority" /> if duplicates occur.
    /// </summary>
    /// <param name="priority">Base dictionary.</param>
    /// <param name="add">Dictionary to add.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>The merged dictionary.</returns>
    internal static Dictionary<TKey, TValue> Merge<TKey, TValue>(
        this Dictionary<TKey, TValue> priority,
        Dictionary<TKey, TValue> add) where TKey : notnull
    {
        var list = priority.ToList();

        list.ForEach(x => add[x.Key] = x.Value);
        return add;
    }
}