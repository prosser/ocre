// <copyright file="AccessibilityComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Comparers;

using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using Ocre.Configuration;

internal class AccessibilityComparer(OcreConfiguration config) : IComparer<AccessibilityConfig>
{
    public int Compare(Accessibility x, Accessibility y) => Compare((AccessibilityConfig)x, (AccessibilityConfig)y);
    public int Compare(AccessibilityConfig x, AccessibilityConfig y)
    {
        // If both are the same, they are equal
        if (x == y)
        {
            return 0;
        }

        int xIndex = Array.IndexOf(config.Accessibility, x);
        int yIndex = Array.IndexOf(config.Accessibility, y);

        // If x is not found in the order array, it is considered greater (comes later)
        if (xIndex == -1)
        {
            return 1;
        }

        // If y is not found in the order array, it is considered greater (comes later)
        return yIndex == -1 ? -1 : xIndex.CompareTo(yIndex);
    }
}
