// <copyright file="AllocationComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

/// <summary>
/// Compares the allocation type of two members, e.g., static vs const vs instance.
/// </summary>
/// <param name="config"></param>
internal class MemberAllocationComparer(OcreConfiguration config) : IComparer<CSharpSyntaxNode>, IComparer<MemberDeclarationSyntax>
{
    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        return Compare((MemberDeclarationSyntax)x, (MemberDeclarationSyntax)y);
    }

    /// <summary>
    /// Compares two members by their allocation type.
    /// </summary>
    public int Compare(MemberDeclarationSyntax x, MemberDeclarationSyntax y)
    {
        // find the allocation type of each member
        AllocationModifierTokenType xAlloc = GetAllocationModifier(x);
        AllocationModifierTokenType yAlloc = GetAllocationModifier(y);

        // If both are the same, they are equal
        if (xAlloc == yAlloc)
        {
            return 0;
        }

        int xIndex = Array.IndexOf(config.AllocationModifierOrder, xAlloc);
        int yIndex = Array.IndexOf(config.AllocationModifierOrder, yAlloc);

        return xIndex == -1
            ? 1
            : yIndex == -1
                ? -1
                : xIndex.CompareTo(yIndex);
    }

    private AllocationModifierTokenType GetAllocationModifier(MemberDeclarationSyntax decl)
    {
        foreach (SyntaxToken modifier in decl.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.StaticKeyword))
            {
                return AllocationModifierTokenType.Static;
            }
            else if (modifier.IsKind(SyntaxKind.ConstKeyword))
            {
                return AllocationModifierTokenType.Const;
            }
        }

        // Default to instance
        return AllocationModifierTokenType.Instance;
    }
}
