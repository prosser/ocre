// <copyright file="OperatorOrder.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

internal enum OperatorKind
{
    Conversion = 0,
    Explicit,
    Unary,
    Binary,
    Arithmetic,
}

