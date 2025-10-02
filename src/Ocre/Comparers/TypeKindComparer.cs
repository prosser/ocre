// <copyright file="TypeKindComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal sealed class TypeKindComparer : IComparer<CSharpSyntaxNode>
{
    private readonly Dictionary<TypeTokenType, int> typeOrderMap;
    public TypeKindComparer(OcreConfiguration config)
    {
        typeOrderMap = [];
        for (int i = 0; i < config.TypeOrder.Length; i++)
        {
            typeOrderMap[config.TypeOrder[i]] = i;
        }
    }

    public int Compare(CSharpSyntaxNode? x, CSharpSyntaxNode? y)
    {
        if (x is null)
        {
            throw new ArgumentNullException(nameof(x));
        }

        if (y is null)
        {
            throw new ArgumentNullException(nameof(y));
        }

        TypeTokenType tx = GetTypeTokenType(x);
        TypeTokenType ty = GetTypeTokenType(y);
        if (tx == ty)
        {
            return 0;
        }

        int xIndex = typeOrderMap.TryGetValue(tx, out int xi) ? xi : int.MaxValue;
        int yIndex = typeOrderMap.TryGetValue(ty, out int yi) ? yi : int.MaxValue;
        return xIndex.CompareTo(yIndex);
    }

    private static TypeTokenType GetTypeTokenType(CSharpSyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax => TypeTokenType.Class,
            StructDeclarationSyntax => TypeTokenType.Struct,
            InterfaceDeclarationSyntax => TypeTokenType.Interface,
            EnumDeclarationSyntax => TypeTokenType.Enum,
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) => TypeTokenType.Record,
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => TypeTokenType.RecordStruct,
            DelegateDeclarationSyntax => TypeTokenType.Delegate,
            _ => throw new ArgumentException("Node is not a type declaration", nameof(node)),
        };
    }
}