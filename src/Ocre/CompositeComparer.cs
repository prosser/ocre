// <copyright file="CompositeComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;

using System.Collections.Generic;
using System.Collections.Immutable;

internal sealed class CompositeComparer<T> : IComparer<T>
{
    private readonly ImmutableArray<IComparer<T>> _parts;
    public CompositeComparer(IEnumerable<IComparer<T>> parts) => _parts = parts.ToImmutableArray();

    public int Compare(T? x, T? y)
    {
        foreach (IComparer<T> c in _parts)
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

