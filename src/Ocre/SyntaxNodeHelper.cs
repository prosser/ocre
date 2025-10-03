// <copyright file="SyntaxNodeHelper.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class SyntaxNodeHelper
{
    public static bool IsTypeNode(this SyntaxNode n) => n is TypeDeclarationSyntax or EnumDeclarationSyntax or DelegateDeclarationSyntax;

    public static IEnumerable<CSharpSyntaxNode> GetTypeDeclarations(this SyntaxNode root) => root
        .ChildNodes()
        .Where(IsTypeNode)
        .Cast<CSharpSyntaxNode>();

    public static IEnumerable<CSharpSyntaxNode> GetTypeDeclarations(this IEnumerable<SyntaxNode> nodes) => nodes.SelectMany(GetTypeDeclarations);

    public static IEnumerable<CSharpSyntaxNode> GetTypeDeclaractionsInFile(this SyntaxNode root)
    {
        // Types declared directly in the compilation unit (global namespace)
        IEnumerable<CSharpSyntaxNode> direct = root.GetTypeDeclarations();

        // Types in block-scoped namespaces
        IEnumerable<CSharpSyntaxNode> blockNs = root.ChildNodes()
            .OfType<NamespaceDeclarationSyntax>()
            .SelectMany(n => n.GetTypeDeclarations());

        // Types in file-scoped namespaces
        IEnumerable<CSharpSyntaxNode> fileScopedNs = root.ChildNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .SelectMany(n => n.GetTypeDeclarations());

        return direct.Concat(blockNs).Concat(fileScopedNs);
    }

    public static string GetDisplayName(this CSharpSyntaxNode node)
    {
        return node switch
        {
            ClassDeclarationSyntax cds => cds.Identifier.Text,
            StructDeclarationSyntax sds => sds.Identifier.Text,
            InterfaceDeclarationSyntax ids => ids.Identifier.Text,
            EnumDeclarationSyntax eds => eds.Identifier.Text,
            RecordDeclarationSyntax rds => rds.Identifier.Text,
            DelegateDeclarationSyntax dds => dds.Identifier.Text,
            NamespaceDeclarationSyntax nds => nds.Name.ToString(),
            _ => node.Kind().ToString(),
        };
    }
}
