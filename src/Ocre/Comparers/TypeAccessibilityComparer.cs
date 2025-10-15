// <copyright file="TypeAccessibilityComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class TypeAccessibilityComparer(OcreConfiguration config) : IComparer<CSharpSyntaxNode>, IComparer<TypeDeclarationSyntax>
{
    private readonly AccessibilityComparer accessibilityComparer = new(config);

    public int Compare(CSharpSyntaxNode x, CSharpSyntaxNode y)
    {
        return Compare((TypeDeclarationSyntax)x, (TypeDeclarationSyntax)y);
    }

    public int Compare(TypeDeclarationSyntax x, TypeDeclarationSyntax y)
    {
        if (x is null)
        {
            throw new ArgumentNullException(nameof(x));
        }

        if (y is null)
        {
            throw new ArgumentNullException(nameof(y));
        }

        Accessibility ax = GetTypeAccessibility(x);
        Accessibility ay = GetTypeAccessibility(y);

        return accessibilityComparer.Compare((AccessibilityConfig)ax, (AccessibilityConfig)ay);
    }

    private static Accessibility GetTypeAccessibility(TypeDeclarationSyntax node)
    {
        SyntaxTokenList mods = node.Modifiers;
        bool isPublic = mods.Any(SyntaxKind.PublicKeyword);
        bool isInternal = mods.Any(SyntaxKind.InternalKeyword);
        bool isProtected = mods.Any(SyntaxKind.ProtectedKeyword);
        bool isPrivate = mods.Any(SyntaxKind.PrivateKeyword);

        if (isPublic)
        {
            return Accessibility.Public;
        }

        if (isProtected && isInternal)
        {
            return Accessibility.ProtectedOrInternal;
        }

        if (isProtected && isPrivate)
        {
            return Accessibility.ProtectedAndInternal;
        }

        if (isInternal)
        {
            return Accessibility.Internal;
        }

        if (isProtected)
        {
            return Accessibility.Protected;
        }

        if (isPrivate)
        {
            return Accessibility.Private;
        }

        // Default for top-level types is internal; for nested types default is private
        return node.Parent is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax
            ? Accessibility.Internal
            : Accessibility.Private;
    }
}
