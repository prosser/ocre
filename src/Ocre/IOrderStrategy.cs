// <copyright file="IOrderStrategy.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

internal interface IOrderStrategy<in TNode> where TNode : SyntaxNode
{
    IComparer<TNode> NodeComparer { get; }
    bool IsBarrier(SyntaxNode node); // e.g., preprocessor/region boundaries
}
