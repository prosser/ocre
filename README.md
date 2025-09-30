# OCRE `.editorconfig` Settings

This document describes the `dotnet_code_quality.ocre.*` settings used for controlling member and operator sort order in C# code.

---

## General Parsing Rules

The top-level sorting strategy is configured by [dotnet_code_quality.ocre.root_order](#dotnet_code_quality.ocre.root_order).
All sorting strategies work in a "then by" way; i.e., the first sorting strategy is executed, and later strategies are performed only when there is equality according to the prior strategy.

This is equivalent to sorting pseudo-code like this:

```csharp
var sorted = unsorted.OrderBy(strategies[0])
    .ThenBy(strategies[1])
    .ThenBy(strategies[2]);
    // ...
```
For all array-type settings:

1. Values are parsed **left-to-right**.
2. Values are **comma-separated**.
3. **Whitespace** and the underscore (`_`) character are removed during parsing.

   * Example: `"member_kind"` == `"member kind"` == `"memberkind"`.
4. Parsing is **case-insensitive**.
5. **Duplicates** are ignored (the first instance is used).
6. **Missing values** are controlled by [dotnet_code_quality.ocre.add_missing_order_values](#dotnet_code_qualityocreadd_missing_order_values).

---

## Settings

### `dotnet_code_quality.ocre.accessibility_order`

Defines the order in which members are sorted according to their declared accessibility.

* **`public`** → Accessible everywhere.
* **`internal`** → Accessible only within the same assembly.
* **`protected internal`** → Accessible within the same assembly or derived types.
* **`protected`** → Accessible only within the class or its derived types.
* **`private`** → Accessible only within the containing type.

**Default:**
`dotnet_code_quality.ocre.accessibility_order = public, internal, protected internal, protected, private`

---

### `dotnet_code_quality.ocre.add_missing_order_values`

Controls how missing values in sort-order settings are handled.

* **`true`** → Missing values are appended in their default relative order.
* **`false`** → Missing values are omitted (the sort dimension is not applied).

**Default:**
`dotnet_code_quality.ocre.add_missing_order_values = true`

---

### `dotnet_code_quality.ocre.allocation_modifier_order`

Defines the order in which members are sorted according to their allocation modifier.

* **`const`** → Constant values known at compile time.
* **`static`** → Members that belong to the type itself, not an instance.
* **`instance`** → Members that belong to a specific object instance (no modifier).

**Default:**
`dotnet_code_quality.ocre.allocation_modifier_order = const, static, instance`

---

### `dotnet_code_quality.ocre.member_order`

Defines the order in which **member kinds** are sorted within a type.

* **`field`** → Variable storage defined at type level.
* **`constructor`** → Type initializers.
* **`event`** → Event declarations.
* **`property`** → Properties with getters and/or setters.
* **`operator`** → Overloaded operators (e.g., `+`, `==`).
* **`method`** → Regular methods.
* **`type`** → Nested types (e.g., inner classes, structs, enums).

**Default:**
`dotnet_code_quality.ocre.member_order = field, constructor, event, property, operator, method, type`

---

### `dotnet_code_quality.ocre.operator_order`

Defines the order in which **categories of operators** are sorted.

* **`conversions`** → Implicit and explicit conversion operators.
* **`unary`** → Operators with a single operand (e.g., `!x`, `++x`).
* **`binary`** → Operators with two operands (e.g., `x + y`).

**Default:**
`dotnet_code_quality.ocre.operator_order = conversions, unary, binary`

---

### `dotnet_code_quality.ocre.operator_order_binary`

Defines the order in which **binary operators** are sorted.

* Arithmetic: `+`, `-`, `*`, `/`, `%`
* Bitwise: `&`, `|`, `^`, `<<`, `>>`
* **`return type`** → Orders by the declared return type of the operator.
* **`arg type`** → Orders by the argument types of the operator.

**Default:**
`dotnet_code_quality.ocre.operator_order_binary = +, -, *, /, %, &, |, ^, <<, >>, return type, arg type`

---

### `dotnet_code_quality.ocre.operator_order_conversion`

Defines the order in which **conversion operators** are sorted.

* **`implicit`** → `implicit operator` definitions.
* **`explicit`** → `explicit operator` definitions.
* **`return type`** → Orders by the declared return type of the operator.
* **`arg 1 type`** → Orders by the first argument type (source type).
* **`arg 2 type`** → Orders by the second argument type (target type).

**Default:**
`dotnet_code_quality.ocre.operator_order_conversion = implicit, explicit, return type, arg 1 type, arg 2 type`

---

### `dotnet_code_quality.ocre.operator_order_unary`

Defines the order in which **unary operators** are sorted.

* Arithmetic: `+`, `-`
* Logical/bitwise: `!`, `~`
* Increment/decrement: `++`, `--`
* Boolean: `true`, `false`
* **`return type`** → Orders by the declared return type of the operator.
* **`arg type`** → Orders by the argument type of the operator.

**Default:**
`dotnet_code_quality.ocre.operator_order_unary = +, -, !, ~, ++, --, true, false, return type, arg type`

---

### `dotnet_code_quality.ocre.root_order`

Defines the order in which **strategies of sorts** are applied at the root of a type being sorted.

* **`member kind`** → [member_order](#dotnet_code_quality.ocre.member_order) strategy.
* **`accessibility`** → [accessibility_order](#dotnet_code_quality.ocre.accessibility_order) strategy.
* **`allocation`** → [allocation_modifier_order](#dotnet_code_quality.ocre.allocation_modifier_order) strategy.
* **`name`** → Alphabetical or lexical ordering by identifier.

**Default:**
`dotnet_code_quality.ocre.root_order = member kind, accessibility, allocation, name`

---

### `dotnet_code_quality.ocre.type_order`

Defines the order in which **types** are sorted at the file level or when nested inside another type.

* **`delegate`** → Delegate types.
* **`enum`** → Enumerations.
* **`interface`** → Interface types.
* **`struct`** → Struct types.
* **`record`** → Record types.
* **`class`** → Class types.
* **`name`** → Alphabetical or lexical ordering by identifier.

**Default:**
`dotnet_code_quality.ocre.type_order = delegate, enum, interface, struct, record, class, name`

---

## Defaults Quick Reference

```
dotnet_code_quality.ocre.accessibility_order = public, internal, protected internal, protected, private
dotnet_code_quality.ocre.add_missing_order_values = true
dotnet_code_quality.ocre.allocation_modifier_order = const, static, instance
dotnet_code_quality.ocre.member_order = field, constructor, event, property, operator, method, type
dotnet_code_quality.ocre.operator_order = conversions, unary, binary
dotnet_code_quality.ocre.operator_order_binary = +, -, *, /, %, &, | , ^, <<, >>, return type, arg 
dotnet_code_quality.ocre.operator_order_conversion = implicit, explicit, return type, arg 1 type, arg 2 type
dotnet_code_quality.ocre.operator_order_unary = +, -, !, ~, ++, --, true, false, return type, arg type
dotnet_code_quality.ocre.root_order = member kind, accessibility, allocation, name
dotnet_code_quality.ocre.type_order = delegate, enum, interface, struct, record, class, name
```
