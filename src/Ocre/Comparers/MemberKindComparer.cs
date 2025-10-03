// <copyright file="MemberKindComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;

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

    private static MemberKind GetMemberKind(CSharpSyntaxNode node) =>
        node.Kind() switch
        {
            SyntaxKind.FieldDeclaration => MemberKind.Field,
            SyntaxKind.ConstructorDeclaration => MemberKind.Constructor,
            SyntaxKind.EventDeclaration or SyntaxKind.EventFieldDeclaration => MemberKind.Event,
            SyntaxKind.PropertyDeclaration or SyntaxKind.IndexerDeclaration => MemberKind.Property,
            SyntaxKind.OperatorDeclaration or SyntaxKind.ConversionOperatorDeclaration => MemberKind.Operator,
            SyntaxKind.MethodDeclaration => MemberKind.Method,
            SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or
            SyntaxKind.InterfaceDeclaration or SyntaxKind.RecordDeclaration or
            SyntaxKind.RecordStructDeclaration or SyntaxKind.DelegateDeclaration => MemberKind.Type,
            _ => throw new ArgumentException($"Unsupported syntax node type: {node.GetType().Name}", nameof(node)),
        };
}
