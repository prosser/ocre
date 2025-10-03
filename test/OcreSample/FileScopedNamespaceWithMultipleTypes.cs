// <copyright file="FileWithMultipleTypes.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

#pragma warning disable IDE0160 // Convert to block scoped namespace
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace OcreSample.FileScoped;
#pragma warning restore IDE0130 // Namespace does not match folder structure
#pragma warning restore IDE0160 // Convert to block scoped namespace

/// <summary>
/// FooEnum is an example enumeration. If you "Sort types", this comment should stay with the enum.
/// </summary>
public enum FooEnum
{
    None = 1,
    Some,
    All,
}

internal class AnotherClass
{
}

internal class SomeClass
{
}
