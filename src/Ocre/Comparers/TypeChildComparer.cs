// <copyright file="TypeChildComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;

using Ocre.Configuration;

internal class TypeChildComparer : IComparer<CSharpSyntaxNode>
{
    private readonly CompositeComparer<CSharpSyntaxNode> comparer;

    public TypeChildComparer(OcreConfiguration config)
    {
        List<IComparer<CSharpSyntaxNode>> comparers = [];

        foreach (SortStrategyKind ssk in config.SortOrder)
        {
            comparers.Add(ssk switch
            {
                SortStrategyKind.Accessibility => new MemberAccessibilityComparer(config),
                SortStrategyKind.Allocation => new MemberAllocationComparer(config),
                SortStrategyKind.MemberKind => new MemberKindComparer(config),
                SortStrategyKind.Name => new MemberNameComparer(),
                _ => throw new NotSupportedException($"Unsupported sort strategy kind: {ssk}"),
            });
        }

        comparer = new(comparers);
    }

    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        return comparer.Compare(x, y);
    }
}
