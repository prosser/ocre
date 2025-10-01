// <copyright file="MemberKindComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

using Microsoft.CodeAnalysis.CSharp;

using Ocre.Comparers;
using Ocre.Configuration;

using Xunit;

public class MemberKindComparerTests
{
    [Fact]
    public void OrdersAccordingToConfig()
    {
        var cfg = new OcreConfiguration
        {
            MemberOrder = [
                MemberKind.Field,
                MemberKind.Constructor,
                MemberKind.Event,
                MemberKind.Property,
                MemberKind.Operator,
                MemberKind.Method,
                MemberKind.Type
            ]
        };

        CSharpSyntaxNode field = SyntaxFactory.ParseMemberDeclaration("int f;");
        CSharpSyntaxNode ctor = SyntaxFactory.ParseMemberDeclaration("public C() { }");
        CSharpSyntaxNode ev = SyntaxFactory.ParseMemberDeclaration("public event System.EventHandler E; ");
        CSharpSyntaxNode prop = SyntaxFactory.ParseMemberDeclaration("public int P { get; set; }");
        CSharpSyntaxNode op = SyntaxFactory.ParseMemberDeclaration("public static C operator +(C a, C b) => a; ");
        CSharpSyntaxNode method = SyntaxFactory.ParseMemberDeclaration("public void M() { }");
        CSharpSyntaxNode type = SyntaxFactory.ParseMemberDeclaration("class D { }");

        var cmp = new MemberKindComparer(cfg);

        Assert.True(cmp.Compare(field, ctor) < 0);
        Assert.True(cmp.Compare(ctor, ev) < 0);
        Assert.True(cmp.Compare(ev, prop) < 0);
        Assert.True(cmp.Compare(prop, op) < 0);
        Assert.True(cmp.Compare(op, method) < 0);
        Assert.True(cmp.Compare(method, type) < 0);
    }
}
