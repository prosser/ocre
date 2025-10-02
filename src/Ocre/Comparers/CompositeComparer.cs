// <copyright file="CompositeComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System.Collections.Generic;
using System.Collections.Immutable;

/// <summary>
/// A comparer that calls a sequence of comparers in order, returning the first non-zero result.
/// </summary>
/// <typeparam name="T">The type of item being compared.</typeparam>
/// <param name="parts">The comparers to use.</param>
internal sealed class CompositeComparer<T>(IEnumerable<IComparer<T>> parts) : IComparer<T>
{
    private readonly ImmutableArray<IComparer<T>> parts = [.. parts];

    /// <summary>
    /// Returns the first non-zero comparison result from the sequence of comparers.
    /// </summary>
    /// <param name="x">The first item to compare.</param>
    /// <param name="y">The second item to compare.</param>
    /// <returns>The comparison result: -1 for x < y, 1 for x > y, 0 for x == y.</y></returns>
    public int Compare(T? x, T? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1; // null is less than non-null
        }

        if (y is null)
        {
            return 1; // non-null is greater than null
        }

        foreach (IComparer<T> c in parts)
        {
            int r = c.Compare(x!, y!);
            if (r != 0)
            {
                return r;
            }
        }

        return 0; // stable: original index preserved by stable sort
    }
}
