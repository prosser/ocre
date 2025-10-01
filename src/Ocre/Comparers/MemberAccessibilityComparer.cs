// <copyright file="MemberAccessibilityComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Configuration;

internal class MemberAccessibilityComparer(OcreConfiguration config) : IComparer<MemberDeclarationSyntax>
{
    private readonly AccessibilityComparer accessibilityComparer = new(config);

    public int Compare(MemberDeclarationSyntax x, MemberDeclarationSyntax y)
    {
        Accessibility ax = GetMemberAccessibility(x);
        Accessibility ay = GetMemberAccessibility(y);

        return accessibilityComparer.Compare(ax, ay);
    }

    private static Accessibility GetMemberAccessibility(MemberDeclarationSyntax node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        SyntaxTokenList mods = node.Modifiers;
        bool isPublic = mods.Any(t => t.IsKind(SyntaxKind.PublicKeyword));
        bool isInternal = mods.Any(t => t.IsKind(SyntaxKind.InternalKeyword));
        bool isProtected = mods.Any(t => t.IsKind(SyntaxKind.ProtectedKeyword));
        bool isPrivate = mods.Any(t => t.IsKind(SyntaxKind.PrivateKeyword));

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

        // If member is declared in an interface, members are implicitly public
        if (node.Parent is InterfaceDeclarationSyntax)
        {
            return Accessibility.Public;
        }

        // Default for members in classes/structs is private
        return Accessibility.Private;
    }
}
