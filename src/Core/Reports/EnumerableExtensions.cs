using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Reports;

public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T, TKey>(
        this IEnumerable<T> source, 
        Func<T, TKey> keySelector, 
        int count)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        return source.OrderByDescending(keySelector).Take(count);
    }
}