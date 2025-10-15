// <copyright file="AccessibilityComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using Analyzers.Ocre.Comparers;
using Analyzers.Ocre.Configuration;

using Microsoft.CodeAnalysis;

using Xunit;

public class AccessibilityComparerTests
{
    [Fact]
    public void OrdersAccordingToConfig()
    {
        var cfg = new OcreConfiguration
        {
            Accessibility = [AccessibilityConfig.Public, AccessibilityConfig.Private]
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
            Accessibility = [AccessibilityConfig.Public]
        };

        var cmp = new AccessibilityComparer(cfg);

        // AccessibilityConfig.Internal is not in the configured order -> treated as "later" than configured values
        Assert.True(cmp.Compare(Accessibility.Public, Accessibility.Internal) < 0);
        Assert.True(cmp.Compare(Accessibility.Internal, Accessibility.Public) > 0);
    }
}
