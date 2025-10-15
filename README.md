# OCRE `.editorconfig` Settings

Obsessive Code Reorganization Engine (OCRE) is a Roslyn analyzer + code fix that detects improperly sorted C# code and automatically sorts it according to user-defined preferences. Configuration is via `.editorconfig` settings.

---

## General Parsing Rules

At the file level, [types](#csharp_style_oc_types) is applied to sort types within the file.
Within each type, members are sorted according to the strategies defined in [members](#csharp_style_oc_members).

All sorting strategies compose like successive `ThenBy` operations.

For all array-type settings:

1. Parsed left-to-right
2. Comma-separated
3. Whitespace and `_` removed ("member_kind" == "member kind" == "memberkind")
4. Case-insensitive
5. Duplicates ignored (first wins)
6. Missing values behavior controlled by [csharp_style_oc_add_missing_order_values](#csharp_style_oc_add_missing_order_values)

---

## Settings

All settings use the `csharp_style_oc_` prefix.

### `csharp_style_oc_accessibility`
Order of member accessibility.
Default:
`csharp_style_oc_accessibility = public,internal,protected internal,protected,private protected,private`

### `csharp_style_oc_add_missing_order_values`
If true, unspecified values are appended in default relative order; otherwise that dimension is skipped for unspecified values.
Default:
`csharp_style_oc_add_missing_order_values = true`

### `csharp_style_oc_allocation_modifiers`
Order of allocation modifiers (`const`, `static`, implicit instance).
Default:
`csharp_style_oc_allocation_modifiers = const,static,instance`

### `csharp_style_oc_member_kinds`
Order of member kinds inside a type.
Default:
`csharp_style_oc_member_kinds = field,constructor,event,property,operator,method,type`

### `csharp_style_oc_operators`
Order of operator categories.
Default:
`csharp_style_oc_operators = conversion,unary,binary`

### `csharp_style_oc_binary_operators`
Ordering phases for binary operators.
Default:
`csharp_style_oc_binary_operators = +,-,*,/,%,&,|,^,<<,>>,parameter`

### `csharp_style_oc_conversion_operators`
Ordering phases for conversion operators.
Default:
`csharp_style_oc_conversion_operators = implicit,explicit`

### `csharp_style_oc_unary_operators`
Ordering phases for unary operators.
Default:
`csharp_style_oc_unary_operators = +,-,!,~,++,--,true,false`

### `csharp_style_oc_parameters`
Ordering for parameter-based comparisons when sorting members or overloads.
* `count` — sort by number of parameters (ascending)
* `types` — sort by parameter type names (lexicographically)

Default:
`csharp_style_oc_parameters = count,types`

### `csharp_style_oc_members`
Sequence of strategies when sorting members. Each subsequent strategy breaks ties from the previous one.
Default:
`csharp_style_oc_members = member_kinds,accessibility,allocation_modifiers,name,parameters,return_type`

### `csharp_style_oc_types`
Order of types at file scope or nested.
Default:
`csharp_style_oc_types = delegate,enum,interface,struct,record_struct,record,class,name`

---

## Diagnostics

OC1000: Types should be sorted correctly in the file.
OC1001: Nested types should be sorted correctly within their containing types.
OC1002: Type members should be sorted correctly.
OC1003: Operators should be sorted correctly.

Configure severity with `dotnet_diagnostic.<ID>.severity`.

---

## Defaults Quick Reference
```
csharp_style_oc_accessibility = public,internal,protected internal,protected,private protected,private
csharp_style_oc_add_missing_order_values = true
csharp_style_oc_allocation_modifiers = const,static,instance
csharp_style_oc_member_kinds = field,constructor,event,property,operator,method,type
csharp_style_oc_operators = conversion,unary,binary
csharp_style_oc_binary_operators = +,-,*,/,%,&,|,^,<<,>>,parameter
csharp_style_oc_conversion_operators = implicit,explicit
csharp_style_oc_unary_operators = +,-,!,~,++,--,true,false
csharp_style_oc_parameters = count,types
csharp_style_oc_members = member_kinds,accessibility,allocation modifiers,name,parameters,return type
csharp_style_oc_types = delegate,enum,interface,struct,record struct,record,class,name
```