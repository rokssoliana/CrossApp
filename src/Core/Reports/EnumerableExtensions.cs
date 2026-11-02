using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

/// <summary>
/// Допоміжні методи-розширення для LINQ to Objects (Додаткове завдання №2).
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Власний generic-метод розширення для детермінованого відбору Top-N елементів.
    /// </summary>
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        Func<T, string> uniqueKeySelector,
        int count)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        if (uniqueKeySelector == null) throw new ArgumentNullException(nameof(uniqueKeySelector));

        return source
            .OrderByDescending(keySelector)
            .ThenBy(uniqueKeySelector, StringComparer.Ordinal)
            .Take(count);
    }
}