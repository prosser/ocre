// <copyright file="UnaryOperatorComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using System;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Xunit;

public class UnaryOperatorComparerTests
{
    [Fact]
    public void OrdersByOperator()
    {
        var cfg = new OcreConfiguration
        {
            UnaryOperatorOrder = [UnaryOperatorTokenType.Plus, UnaryOperatorTokenType.Minus, UnaryOperatorTokenType.Negate, UnaryOperatorTokenType.Complement, UnaryOperatorTokenType.Increment, UnaryOperatorTokenType.Decrement, UnaryOperatorTokenType.True, UnaryOperatorTokenType.False]
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
