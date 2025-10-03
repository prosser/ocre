// <copyright file="TypeDeclarationComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;
using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

public class TypeDeclarationComparer(OcreConfiguration config, SemanticModel? semanticModel) : IComparer<CSharpSyntaxNode>
{
    private readonly TypeNameComparer nameComparer = new(semanticModel);

    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        TypeTokenType tx = GetTypeTokenType(x);
        TypeTokenType ty = GetTypeTokenType(y);

        int cmp = 0;
        for (int i = 0; i < config.TypeOrder.Length && cmp == 0; i++)
        {
            TypeTokenType tCur = config.TypeOrder[i];
            if (tCur == TypeTokenType.Name)
            {
                cmp = nameComparer.Compare(x, y);
            }
            else if (tx != ty)
            {
                if (tCur == tx)
                {
                    cmp = -1;
                }
                else if (tCur == ty)
                {
                    cmp = 1;
                }
            }
        }

        return cmp;
    }

    private static TypeTokenType GetTypeTokenType(CSharpSyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax => TypeTokenType.Class,
            StructDeclarationSyntax => TypeTokenType.Struct,
            InterfaceDeclarationSyntax => TypeTokenType.Interface,
            EnumDeclarationSyntax => TypeTokenType.Enum,
            // Explicit record class
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) => TypeTokenType.Record,
            // Explicit record struct
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => TypeTokenType.RecordStruct,
            // Implicit record class (no class/struct modifier token present)
            RecordDeclarationSyntax => TypeTokenType.Record,
            DelegateDeclarationSyntax => TypeTokenType.Delegate,
            _ => throw new ArgumentException("Node is not a type declaration", nameof(node)),
        };
    }
}
