// <copyright file="MemberKindComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Ocre.Configuration;

internal class MemberKindComparer(OcreConfiguration config, SemanticModel? semanticModel) : IComparer<CSharpSyntaxNode>
{
    private readonly CompositeComparer<CSharpSyntaxNode> operatorComparer = new(
        config.Operators.Select(k => k switch
        {
            OperatorsConfig.Unary => (IComparer<CSharpSyntaxNode>)new UnaryOperatorComparer(config, semanticModel),
            OperatorsConfig.Binary => new BinaryOperatorComparer(config, semanticModel),
            _ => throw new NotSupportedException($"Unsupported operator kind: {k}"),
        }));
    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        // Extract the kind of each member.
        MemberKindsConfig xKind = GetMemberKind(x);
        MemberKindsConfig yKind = GetMemberKind(y);

        if (xKind == yKind)
        {
            return xKind switch
            {
                MemberKindsConfig.Operator => operatorComparer.Compare(x, y),
                // TODO: add sorting strategies like "by number of parameters" for methods
                _ => 0,
            };
        }

        int xIndex = Array.IndexOf(config.MemberKinds, xKind);
        int yIndex = Array.IndexOf(config.MemberKinds, yKind);

        // If x is not found in the order array, it is considered greater (comes later).
        if (xIndex == -1)
        {
            return 1;
        }

        // If y is not found in the order array, it is considered greater (comes later).
        return yIndex == -1 ? -1 : xIndex.CompareTo(yIndex);
    }

    private static MemberKindsConfig GetMemberKind(CSharpSyntaxNode node) =>
        node.Kind() switch
        {
            SyntaxKind.FieldDeclaration => MemberKindsConfig.Field,
            SyntaxKind.ConstructorDeclaration => MemberKindsConfig.Constructor,
            SyntaxKind.EventDeclaration or SyntaxKind.EventFieldDeclaration => MemberKindsConfig.Event,
            SyntaxKind.PropertyDeclaration or SyntaxKind.IndexerDeclaration => MemberKindsConfig.Property,
            SyntaxKind.OperatorDeclaration or SyntaxKind.ConversionOperatorDeclaration => MemberKindsConfig.Operator,
            SyntaxKind.MethodDeclaration => MemberKindsConfig.Method,
            SyntaxKind.ClassDeclaration or SyntaxKind.StructDeclaration or
            SyntaxKind.InterfaceDeclaration or SyntaxKind.RecordDeclaration or
            SyntaxKind.RecordStructDeclaration or SyntaxKind.DelegateDeclaration => MemberKindsConfig.Type,
            _ => (MemberKindsConfig)(-1),
        };
}
