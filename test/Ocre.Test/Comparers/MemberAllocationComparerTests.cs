// <copyright file="MemberAllocationComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using System;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

public class MemberAllocationComparerTests
{
    private static T ParseMember<T>(string code) where T : MemberDeclarationSyntax =>
        SyntaxFactory.ParseMemberDeclaration(code) as T
        ?? throw new InvalidOperationException();

    private static FieldDeclarationSyntax ParseField(string code) => ParseMember<FieldDeclarationSyntax>(code);
    private static PropertyDeclarationSyntax ParseProperty(string code) => ParseMember<PropertyDeclarationSyntax>(code);

    [Fact]
    public void OrdersConstStaticInstance()
    {
        var cfg = new OcreConfiguration
        {
            AllocationModifierOrder = [AllocationModifierTokenType.Const, AllocationModifierTokenType.Static, AllocationModifierTokenType.Instance]
        };

        FieldDeclarationSyntax constField = ParseField("private const int A = 1;");
        FieldDeclarationSyntax staticField = ParseField("private static int B; ");
        PropertyDeclarationSyntax instanceProp = ParseProperty("private int C { get; set; }");

        var cmp = new MemberAllocationComparer(cfg);

        Assert.True(cmp.Compare(constField, staticField) < 0);
        Assert.True(cmp.Compare(staticField, instanceProp) < 0);
        Assert.True(cmp.Compare(constField, instanceProp) < 0);
    }

    [Fact]
    public void MissingValueBehavior()
    {
        var cfg = new OcreConfiguration
        {
            AllocationModifierOrder = [AllocationModifierTokenType.Static]
        };

        PropertyDeclarationSyntax instanceProp = ParseProperty("private int C { get; set; }");
        FieldDeclarationSyntax staticField = ParseField("private static int B; ");

        var cmp = new MemberAllocationComparer(cfg);

        // instance not in config -> treated as later than static
        Assert.True(cmp.Compare(staticField, instanceProp) < 0);
    }
}
