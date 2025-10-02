// <copyright file="TypeNameComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class TypeNameComparer(SemanticModel? semanticModel) : IComparer<CSharpSyntaxNode>
{
    private readonly SemanticModel? semanticModel = semanticModel;
    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        string sx = GetTypeSymbol(x)?.Name ?? GetSyntaxTypeName(x);
        string sy = GetTypeSymbol(y)?.Name ?? GetSyntaxTypeName(y);

        return StringComparer.Ordinal.Compare(sx, sy);
    }

    private string GetSyntaxTypeName(SyntaxNode x)
    {
        return x switch
        {
            ClassDeclarationSyntax cds => cds.Identifier.Text,
            StructDeclarationSyntax sds => sds.Identifier.Text,
            InterfaceDeclarationSyntax ids => ids.Identifier.Text,
            RecordDeclarationSyntax rds => rds.Identifier.Text,
            EnumDeclarationSyntax eds => eds.Identifier.Text,
            DelegateDeclarationSyntax dds => dds.Identifier.Text,
            _ => string.Empty,
        };
    }

    private INamedTypeSymbol? GetTypeSymbol(SyntaxNode node)
    {
        return semanticModel?.GetDeclaredSymbol(node) as INamedTypeSymbol;
    }
}
