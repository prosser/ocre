// <copyright file="FileWithMultipleTypes.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace OcreSample
{
    /// <summary>
    /// FooEnum is an example enumeration. If you "Sort types", this comment should stay with the enum.
    /// </summary>
    public enum FooEnum
    {
        /// <summary>
        /// Some trivia that should stay with None.
        /// </summary>
        None = 1,
        Some,
        All,
    }

    /// <summary>
    /// AnotherClass is another example class. If you "Sort types", this comment should stay with the class.
    /// </summary>
    internal class AnotherClass
    {
    }

    internal class SomeClass
    {
    }
}
