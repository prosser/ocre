# OCRE `.editorconfig` Settings

**O**bsessive **C**ode **R**eorganization **E**ngine (OCRE) is a Roslyn analyzer and
code fix provider that detects improperly sorted C# code and automatically sorts it
according to user-defined preferences. Configuration is via `.editorconfig` settings.

---

## General Parsing Rules

At the file level, [type_order](#csharp_style_ocre_type_order) is applied to sort types within the file.
Within each type, members are sorted according to the strategies defined in [strategy_order](#csharp_style_ocre_strategy_order).

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
6. **Missing values** are controlled by [csharp_style_ocre_add_missing_order_values](#csharp_style_ocre_add_missing_order_values).

---

## Settings

Settings are read from `.editorconfig` files using the `csharp_style_ocre_` prefix.

### `csharp_style_ocre_accessibility_order`

Defines the order in which members are sorted according to their declared accessibility.

* **`public`** → Accessible everywhere.
* **`internal`** → Accessible only within the same assembly.
* **`protected internal`** → Accessible within the same assembly or derived types.
* **`protected`** → Accessible only within the class or its derived types.
* **`private`** → Accessible only within the containing type.

**Default:**
`csharp_style_ocre_accessibility_order = public, internal, protected internal, protected, private`

---

### `csharp_style_ocre_add_missing_order_values`

Controls how missing values in sort-order settings are handled.

* **`true`** → Missing values are appended in their default relative order.
* **`false`** → Missing values are omitted (the sort dimension is not applied).

**Default:**
`csharp_style_ocre_add_missing_order_values = true`

---

### `csharp_style_ocre_allocation_modifier_order`

Defines the order in which members are sorted according to their allocation modifier.

* **`const`** → Constant values known at compile time.
* **`static`** → Members that belong to the type itself, not an instance.
* **`instance`** → Members that belong to a specific object instance (no modifier).

**Default:**
`csharp_style_ocre_allocation_modifier_order = const, static, instance`

---

### `csharp_style_ocre_member_order`

Defines the order in which **member kinds** are sorted within a type.

* **`field`** → Variable storage defined at type level.
* **`constructor`** → Type initializers.
* **`event`** → Event declarations.
* **`property`** → Properties with getters and/or setters.
* **`operator`** → Overloaded operators (e.g., `+`, `==`).
* **`method`** → Regular methods.
* **`type`** → Nested types (e.g., inner classes, structs, enums).

**Default:**
`csharp_style_ocre_member_order = field, constructor, event, property, operator, method, type`

---

### `csharp_style_ocre_operator_order`

Defines the order in which **categories of operators** are sorted.

* **`conversion`** → Implicit and explicit conversion operators.
* **`unary`** → Operators with a single operand (e.g., `!x`, `++x`).
* **`binary`** → Operators with two operands (e.g., `x + y`).

**Default:**
`csharp_style_ocre_operator_order = conversion, unary, binary`

---

### `csharp_style_ocre_operator_order_binary`

Defines the order in which **binary operators** are sorted.

* Arithmetic: `+`, `-`, `*`, `/`, `%`
* Bitwise: `&`, `|`, `^`, `<<`, `>>`
* **`return type`** → Orders by the declared return type of the operator.
* **`arg 1 type`** / **`arg 2 type`** → Orders by the argument types of the operator.

**Default:**
`csharp_style_ocre_operator_order_binary = +, -, *, /, %, &, |, ^, <<, >>, return type, arg 1 type, arg 2 type`

---

### `csharp_style_ocre_operator_order_conversion`

Defines the order in which **conversion operators** are sorted.

* **`implicit`** → `implicit operator` definitions.
* **`explicit`** → `explicit operator` definitions.
* **`return type`** → Orders by the declared return type of the operator.
* **`arg 1 type`** → Orders by the first argument type (source type).
* **`arg 2 type`** → Orders by the second argument type (target type).

**Default:**
`csharp_style_ocre_operator_order_conversion = implicit, explicit, return type, arg 1 type, arg 2 type`

---

### `csharp_style_ocre_operator_order_unary`

Defines the order in which **unary operators** are sorted.

* Arithmetic: `+`, `-`
* Logical/bitwise: `!`, `~`
* Increment/decrement: `++`, `--`
* Boolean: `true`, `false`
* **`return type`** → Orders by the declared return type of the operator.
* **`arg type`** → Orders by the argument type of the operator.

**Default:**
`csharp_style_ocre_operator_order_unary = +, -, !, ~, ++, --, true, false, return type, arg type`

---

### `csharp_style_ocre_strategy_order`

Defines the order in which strategies are applied for a type being sorted.

* **`member kind`** → [csharp_style_ocre_member_order](#csharp_style_ocre_member_order) strategy.
* **`accessibility`** → [csharp_style_ocre_accessibility_order](#csharp_style_ocre_accessibility_order) strategy.
* **`allocation`** → [csharp_style_ocre_allocation_modifier_order](#csharp_style_ocre_allocation_modifier_order) strategy.
* **`name`** → Alphabetical or lexical ordering by identifier.

**Default:**
`csharp_style_ocre_strategy_order = member kind, accessibility, allocation, name`

---

### `csharp_style_ocre_type_order`

Defines the order in which **types** are sorted at the file level or when nested inside another type.

* **`delegate`** → Delegate types.
* **`enum`** → Enumerations.
* **`interface`** → Interface types.
* **`struct`** → Struct types.
* **`record`** → Record types.
* **`class`** → Class types.
* **`name`** → Alphabetical or lexical ordering by identifier.

**Default:**
`csharp_style_ocre_type_order = delegate, enum, interface, struct, record, class, name`

---

## Diagnostics

Use the following diagnostic IDs in `.editorconfig` to control severities.

* OC1000: Types should be sorted correctly in the file.
* OC1001: Nested types should be sorted correctly within their containing types.
* OC1002: Type members should be sorted correctly.
* OC1003: Operators should be sorted correctly.

You can set per-rule severity with the standard `dotnet_diagnostic.<ID>.severity` key.

---

## Defaults Quick Reference

```
csharp_style_ocre_accessibility_order = public, internal, protected internal, protected, protected private, private
csharp_style_ocre_add_missing_order_values = true
csharp_style_ocre_allocation_modifier_order = const, static, instance
csharp_style_ocre_member_order = field, constructor, event, property, operator, method, type
csharp_style_ocre_operator_order = conversion, unary, binary
csharp_style_ocre_operator_order_binary = +, -, *, /, %, &, | , ^, <<, >>, return type, arg 1 type, arg 2 type
csharp_style_ocre_operator_order_conversion = implicit, explicit, return type, arg 1 type, arg 2 type
csharp_style_ocre_operator_order_unary = +, -, !, ~, ++, --, true, false, return type, arg type
csharp_style_ocre_strategy_order = member kind, accessibility, allocation, name
csharp_style_ocre_type_order = delegate, enum, interface, struct, record, class, name
```