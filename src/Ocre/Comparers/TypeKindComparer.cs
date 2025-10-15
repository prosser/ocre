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
    private readonly Dictionary<TypesConfig, int> typeOrderMap;
    public TypeKindComparer(OcreConfiguration config)
    {
        typeOrderMap = [];
        for (int i = 0; i < config.Types.Length; i++)
        {
            typeOrderMap[config.Types[i]] = i;
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

        TypesConfig tx = GetTypeTokenType(x);
        TypesConfig ty = GetTypeTokenType(y);
        if (tx == ty)
        {
            return 0;
        }

        int xIndex = typeOrderMap.TryGetValue(tx, out int xi) ? xi : int.MaxValue;
        int yIndex = typeOrderMap.TryGetValue(ty, out int yi) ? yi : int.MaxValue;
        return xIndex.CompareTo(yIndex);
    }

    private static TypesConfig GetTypeTokenType(CSharpSyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax => TypesConfig.Class,
            StructDeclarationSyntax => TypesConfig.Struct,
            InterfaceDeclarationSyntax => TypesConfig.Interface,
            EnumDeclarationSyntax => TypesConfig.Enum,
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) => TypesConfig.Record,
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => TypesConfig.RecordStruct,
            DelegateDeclarationSyntax => TypesConfig.Delegate,
            _ => throw new ArgumentException("Node is not a type declaration", nameof(node)),
        };
    }
}