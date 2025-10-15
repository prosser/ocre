// <copyright file="CaseInsensitiveOrdinalComparer.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class CaseInsensitiveOrdinalComparer : IEqualityComparer<string>
{
    public static readonly CaseInsensitiveOrdinalComparer Instance = new(new char[] { '_', ' ' });

    // Depending on the input set we use one of these zero-allocation runtime checks
    private readonly bool useBitmap;
    private readonly bool[]? bitmap;           // allocated once in ctor when beneficial
    private readonly bool useSorted;
    private readonly char[]? sortedIgnored;    // allocated once in ctor when beneficial
    private readonly char[]? smallIgnored;     // small array (<= threshold) used for linear scan

    private const int BinarySearchThreshold = 8;
    private const int AsciiBitmapSize = 128;

    public CaseInsensitiveOrdinalComparer(char[] ignoredCharacters)
    {
        // clone to avoid external mutation
        char[] clone = (char[]?)ignoredCharacters?.Clone() ?? [];

        if (clone.Length == 0)
        {
            useBitmap = false;
            bitmap = null;
            useSorted = false;
            sortedIgnored = null;
            smallIgnored = [];
            return;
        }

        // Normalize ignored characters to upper-case to make the ignored set case-insensitive
        for (int i = 0; i < clone.Length; i++)
        {
            clone[i] = char.ToUpperInvariant(clone[i]);
        }

        // normalize: remove duplicates by sorting and compacting
        Array.Sort(clone);
        int write = 1;
        for (int i = 1; i < clone.Length; i++)
        {
            if (clone[i] != clone[write - 1])
            {
                clone[write++] = clone[i];
            }
        }

        if (write != clone.Length)
        {
            Array.Resize(ref clone, write);
        }

        // If all characters are ASCII and there's more than one, bitmap is fastest
        int max = 0;
        for (int i = 0; i < clone.Length; i++)
        {
            if (clone[i] > max)
            {
                max = clone[i];
            }
        }

        if (max < AsciiBitmapSize && clone.Length >= 2)
        {
            useBitmap = true;
            useSorted = false;
            smallIgnored = null;
            bitmap = new bool[AsciiBitmapSize];
            for (int i = 0; i < clone.Length; i++)
            {
                bitmap[clone[i]] = true;
            }

            sortedIgnored = null;
            return;
        }

        // If set is moderately large, keep sorted and use binary search
        if (clone.Length > BinarySearchThreshold)
        {
            useBitmap = false;
            bitmap = null;
            useSorted = true;
            sortedIgnored = clone; // already sorted and deduplicated and upper-cased
            smallIgnored = null;
            return;
        }

        // Small set: keep as-is for linear scan (already upper-cased)
        useBitmap = false;
        bitmap = null;
        useSorted = false;
        sortedIgnored = null;
        smallIgnored = clone;
    }

    public bool Equals(string x, string y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        int ix = 0, iy = 0;
        int lx = x.Length, ly = y.Length;

        while (ix < lx && iy < ly)
        {
            char cx = x[ix];
            char cy = y[iy];

            if (IsIgnored(cx))
            {
                ix++;
                continue;
            }

            if (IsIgnored(cy))
            {
                iy++;
                continue;
            }

            if (char.ToUpperInvariant(cx) != char.ToUpperInvariant(cy))
            {
                return false;
            }

            ix++;
            iy++;
        }

        // skip any remaining ignored characters
        while (ix < lx && IsIgnored(x[ix]))
        {
            ix++;
        }

        while (iy < ly && IsIgnored(y[iy]))
        {
            iy++;
        }

        return ix == lx && iy == ly;
    }

    public int GetHashCode(string obj)
    {
        if (obj is null)
        {
            return 0;
        }

        unchecked
        {
            int hash = 17;
            foreach (char c in obj)
            {
                if (IsIgnored(c))
                {
                    continue;
                }

                hash = hash * 31 + char.ToUpperInvariant(c);
            }

            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsIgnored(char c)
    {
        char uc = char.ToUpperInvariant(c);

        return useBitmap ? IsIgnoredBitmap(uc) : useSorted ? IsIgnoredSorted(uc) : IsIgnoredSmall(uc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsIgnoredBitmap(char uc)
    {
        int ci = uc;
        return ci < bitmap!.Length && bitmap[ci];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsIgnoredSorted(char uc)
    {
        return Array.BinarySearch(sortedIgnored!, uc) >= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsIgnoredSmall(char uc)
    {
        char[] arr = smallIgnored!;
        for (int i = 0; i < arr.Length; i++)
        {
            if (uc == arr[i])
            {
                return true;
            }
        }

        return false;
    }
}
