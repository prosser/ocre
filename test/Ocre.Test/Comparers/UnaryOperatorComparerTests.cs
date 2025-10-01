// <copyright file="UnaryOperatorComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Ocre.Comparers;
using Ocre.Configuration;

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

        var unaryPlus = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static C operator +(C a) => a;");
        var unaryMinus = (OperatorDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration("public static C operator -(C a) => a;");

        var cmp = new UnaryOperatorComparer(cfg);

        Assert.True(cmp.Compare(unaryPlus, unaryMinus) < 0);
    }
}
