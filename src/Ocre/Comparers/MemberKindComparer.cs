// <copyright file="MemberKindComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class MemberKindComparer(OcreConfiguration config) : IComparer<CSharpSyntaxNode>
{
    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        // Extract the kind of each member.
        MemberKind xKind = GetMemberKind(x);
        MemberKind yKind = GetMemberKind(y);

        // If both are the same kind, they are equal.
        if (xKind == yKind)
        {
            return 0;
        }

        int xIndex = Array.IndexOf(config.MemberOrder, xKind);
        int yIndex = Array.IndexOf(config.MemberOrder, yKind);

        // If x is not found in the order array, it is considered greater (comes later).
        if (xIndex == -1)
        {
            return 1;
        }

        // If y is not found in the order array, it is considered greater (comes later).
        return yIndex == -1 ? -1 : xIndex.CompareTo(yIndex);
    }

    private MemberKind GetMemberKind(CSharpSyntaxNode node)
    {
        if (node is FieldDeclarationSyntax)
        {
            return MemberKind.Field;
        }
        else if (node is ConstructorDeclarationSyntax)
        {
            return MemberKind.Constructor;
        }
        else if (node is EventDeclarationSyntax or EventFieldDeclarationSyntax)
        {
            return MemberKind.Event;
        }
        else if (node is PropertyDeclarationSyntax or IndexerDeclarationSyntax)
        {
            return MemberKind.Property;
        }
        else if (node is OperatorDeclarationSyntax or ConversionOperatorDeclarationSyntax)
        {
            return MemberKind.Operator;
        }
        else if (node is MethodDeclarationSyntax)
        {
            return MemberKind.Method;
        }
        else if (node is TypeDeclarationSyntax or DelegateDeclarationSyntax)
        {
            return MemberKind.Type;
        }
        else
        {
            throw new ArgumentException($"Unsupported syntax node type: {node.GetType().Name}", nameof(node));
        }
    }
}
