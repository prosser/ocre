// <copyright file="CompositeComparerTests.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Test.Comparers;

extern alias Analyzers;

using System.Collections.Generic;

using Analyzers.Ocre.Comparers;

using Xunit;

public class CompositeComparerTests
{
    [Fact]
    public void ComposesParts()
    {
        var parts = new IComparer<int>[] { Comparer<int>.Default };
        var composite = new CompositeComparer<int>(parts);

        Assert.Equal(0, composite.Compare(1, 1));
        Assert.True(composite.Compare(1, 2) < 0);
    }
}
