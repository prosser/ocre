// <copyright file="MemberAllocationComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Comparers;
using Ocre.Configuration;

using Xunit;

public class MemberAllocationComparerTests
{
    [Fact]
    public void OrdersConstStaticInstance()
    {
        var cfg = new OcreConfiguration
        {
            AllocationModifierOrder = [AllocationModifierTokenType.Const, AllocationModifierTokenType.Static, AllocationModifierTokenType.Instance]
        };

        var constField = (FieldDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("private const int A = 1;");
        var staticField = (FieldDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("private static int B; ");
        var instanceProp = (PropertyDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("private int C { get; set; }");

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

        var instanceProp = (PropertyDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("private int C { get; set; }");
        var staticField = (FieldDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("private static int B; ");

        var cmp = new MemberAllocationComparer(cfg);

        // instance not in config -> treated as later than static
        Assert.True(cmp.Compare(staticField, instanceProp) < 0);
    }
}
