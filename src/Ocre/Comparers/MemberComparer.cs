// <copyright file="MemberComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Ocre.Configuration;

/// <summary>
/// Compares members of a type according to configured ordering rules.
/// </summary>
internal class MemberComparer : IComparer<CSharpSyntaxNode>
{
    private readonly CompositeComparer<CSharpSyntaxNode> comparer;

    public MemberComparer(OcreConfiguration config, SemanticModel? semanticModel)
    {
        List<IComparer<CSharpSyntaxNode>> comparers = [];

        foreach (MembersConfig strategy in config.Members)
        {
            comparers.Add(strategy switch
            {
                MembersConfig.Accessibility => new MemberAccessibilityComparer(config),
                MembersConfig.AllocationModifiers => new MemberAllocationComparer(config),
                MembersConfig.MemberKinds => new MemberKindComparer(config, semanticModel),
                MembersConfig.Name => new MemberNameComparer(),
                _ => throw new NotSupportedException($"Unsupported sort strategy: {strategy}"),
            });
        }

        comparer = new(comparers);
    }

    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        return comparer.Compare(x, y);
    }
}
