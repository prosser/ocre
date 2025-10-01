// <copyright file="AccessibilityComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

using Microsoft.CodeAnalysis;

using Ocre.Comparers;
using Ocre.Configuration;

using Xunit;

public class AccessibilityComparerTests
{
    [Fact]
    public void OrdersAccordingToConfig()
    {
        var cfg = new OcreConfiguration
        {
            AccessibilityOrder = [Accessibility.Public, Accessibility.Private]
        };

        var cmp = new AccessibilityComparer(cfg);

        Assert.True(cmp.Compare(Accessibility.Public, Accessibility.Private) < 0);
        Assert.True(cmp.Compare(Accessibility.Private, Accessibility.Public) > 0);
        Assert.Equal(0, cmp.Compare(Accessibility.Public, Accessibility.Public));
    }

    [Fact]
    public void MissingValueBehavior()
    {
        var cfg = new OcreConfiguration
        {
            AccessibilityOrder = [Accessibility.Public]
        };

        var cmp = new AccessibilityComparer(cfg);

        // Accessibility.Internal is not in the configured order -> treated as "later" than configured values
        Assert.True(cmp.Compare(Accessibility.Public, Accessibility.Internal) < 0);
        Assert.True(cmp.Compare(Accessibility.Internal, Accessibility.Public) > 0);
    }
}
