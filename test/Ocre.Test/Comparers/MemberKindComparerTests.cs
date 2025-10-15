// <copyright file="MemberKindComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using System.Linq;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

public class MemberKindComparerTests
{
    [Fact]
    public void OrdersAccordingToConfig()
    {
        var cfg = new OcreConfiguration
        {
            MemberKinds = [
                MemberKindsConfig.Field,
                MemberKindsConfig.Constructor,
                MemberKindsConfig.Event,
                MemberKindsConfig.Property,
                MemberKindsConfig.Operator,
                MemberKindsConfig.Method,
                MemberKindsConfig.Type
            ]
        };

        const string Source = """
            public class C
            {
                int f;
                public C() { }
                public event System.EventHandler E;
                public int P { get; set; }
                public static C operator +(C a, C b) => a;
                public void M() { }
                class D { }
            }
            """;

        SyntaxTree tree = CSharpSyntaxTree.ParseText(Source);
        ClassDeclarationSyntax root = tree.GetRoot().ChildNodes().Single() as ClassDeclarationSyntax ?? throw new System.Exception($"Failed to get root node: {tree.GetRoot()!.Kind()}");
        var field = (FieldDeclarationSyntax)root.Members[0];
        var ctor = (ConstructorDeclarationSyntax)root.Members[1];
        var ev = (EventFieldDeclarationSyntax)root.Members[2];
        var prop =  (PropertyDeclarationSyntax)root.Members[3];
        var binaryAddOp = (OperatorDeclarationSyntax)root.Members[4];
        var method = (MethodDeclarationSyntax)root.Members[5];
        var nestedType = (ClassDeclarationSyntax)root.Members[6];

        var cmp = new MemberKindComparer(cfg, null);

        Assert.True(cmp.Compare(field, ctor) < 0);
        Assert.True(cmp.Compare(ctor, ev) < 0);
        Assert.True(cmp.Compare(ev, prop) < 0);
        Assert.True(cmp.Compare(prop, binaryAddOp) < 0);
        Assert.True(cmp.Compare(binaryAddOp, method) < 0);
        Assert.True(cmp.Compare(method, nestedType) < 0);
    }
}
