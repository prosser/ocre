// <copyright file="MemberKindComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using Microsoft.CodeAnalysis.CSharp;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Xunit;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class MemberKindComparerTests
{
    private static MemberDeclarationSyntax ParseMember(string code)
    {
        return SyntaxFactory.ParseMemberDeclaration(code) ?? throw new System.Exception("Failed to parse member");
    }

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

        MemberDeclarationSyntax field = ParseMember("int f;");
        MemberDeclarationSyntax ctor = ParseMember("public C() { }");
        MemberDeclarationSyntax ev = ParseMember("public event System.EventHandler E; ");
        MemberDeclarationSyntax prop = ParseMember("public int P { get; set; }");
        MemberDeclarationSyntax op = ParseMember("public static C operator +(C a, C b) => a; ");
        MemberDeclarationSyntax method = ParseMember("public void M() { }");
        MemberDeclarationSyntax type = ParseMember("class D { }");

        var cmp = new MemberKindComparer(cfg);

        Assert.True(cmp.Compare(field, ctor) < 0);
        Assert.True(cmp.Compare(ctor, ev) < 0);
        Assert.True(cmp.Compare(ev, prop) < 0);
        Assert.True(cmp.Compare(prop, op) < 0);
        Assert.True(cmp.Compare(op, method) < 0);
        Assert.True(cmp.Compare(method, type) < 0);
    }
}
