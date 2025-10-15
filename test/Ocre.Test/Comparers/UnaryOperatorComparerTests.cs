// <copyright file="UnaryOperatorComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using System;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

public class UnaryOperatorComparerTests
{
    [Fact]
    public void OrdersByOperator()
    {
        var cfg = new OcreConfiguration
        {
            UnaryOperators = [UnaryOperatorConfig.Plus, UnaryOperatorConfig.Minus, UnaryOperatorConfig.Negate, UnaryOperatorConfig.Complement, UnaryOperatorConfig.Increment, UnaryOperatorConfig.Decrement, UnaryOperatorConfig.True, UnaryOperatorConfig.False]
        };

        static OperatorDeclarationSyntax Parse(string code)
            => SyntaxFactory.ParseMemberDeclaration(code) as OperatorDeclarationSyntax
               ?? throw new InvalidOperationException();

        OperatorDeclarationSyntax unaryPlus = Parse("public static C operator +(C a) => a;");
        OperatorDeclarationSyntax unaryMinus = Parse("public static C operator -(C a) => a;");

        var cmp = new UnaryOperatorComparer(cfg);

        Assert.True(cmp.Compare(unaryPlus, unaryMinus) < 0);
    }
}
