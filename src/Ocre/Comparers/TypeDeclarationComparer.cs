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
        TypesConfig tx = GetTypeTokenType(x);
        TypesConfig ty = GetTypeTokenType(y);

        int cmp = 0;
        for (int i = 0; i < config.Types.Length && cmp == 0; i++)
        {
            TypesConfig tCur = config.Types[i];
            if (tCur == TypesConfig.Name)
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

    private static TypesConfig GetTypeTokenType(CSharpSyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax => TypesConfig.Class,
            StructDeclarationSyntax => TypesConfig.Struct,
            InterfaceDeclarationSyntax => TypesConfig.Interface,
            EnumDeclarationSyntax => TypesConfig.Enum,
            // Explicit record class
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.ClassKeyword) => TypesConfig.Record,
            // Explicit record struct
            RecordDeclarationSyntax r when r.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => TypesConfig.RecordStruct,
            // Implicit record class (no class/struct modifier token present)
            RecordDeclarationSyntax => TypesConfig.Record,
            DelegateDeclarationSyntax => TypesConfig.Delegate,
            _ => throw new ArgumentException("Node is not a type declaration", nameof(node)),
        };
    }
}
