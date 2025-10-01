// <copyright file="MemberOrder.cs">Copyright (c) Peter Rosser. All rights reserved.</copyright>

namespace Ocre.Configuration;

internal enum MemberKind
{
    Field = 0,
    Constructor,
    Event,
    Property,
    Operator,
    Method,
    Type,
}

