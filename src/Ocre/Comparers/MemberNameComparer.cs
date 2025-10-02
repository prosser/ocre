// <copyright file="MemberNameComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class MemberNameComparer() : IComparer<CSharpSyntaxNode>
{
    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        string sx = GetMemberName(x);
        string sy = GetMemberName(y);

        return StringComparer.Ordinal.Compare(sx, sy);
    }

    private string GetMemberName(SyntaxNode node)
    {
        return node switch
        {
            FieldDeclarationSyntax fds => string.Join(",", fds.Declaration.Variables.Select(v => v.Identifier.Text)),
            ConstructorDeclarationSyntax cds => cds.Identifier.Text,
            EventDeclarationSyntax eds => eds.Identifier.Text,
            EventFieldDeclarationSyntax efds => string.Join(",", efds.Declaration.Variables.Select(v => v.Identifier.Text)),
            PropertyDeclarationSyntax pds => pds.Identifier.Text,
            IndexerDeclarationSyntax ids => "this",
            MethodDeclarationSyntax mds => mds.Identifier.Text,
            OperatorDeclarationSyntax ods => ods.OperatorToken.Text,
            ConversionOperatorDeclarationSyntax cods => cods.Type.ToString(),
            _ => string.Empty,
        };
    }
}