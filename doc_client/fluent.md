# Open API Fluent Interface

The Deephaven system has numerous methods that take expressions (e.g. <xref:Deephaven.OpenAPI.Client.IQueryTable.Select*> or
<xref:Deephaven.OpenAPI.Client.IQueryTable.View*>), boolean
conditions (e.g. <xref:Deephaven.OpenAPI.Client.IQueryTable.Where*>),
column names (e.g. <xref:Deephaven.OpenAPI.Client.IQueryTable.Sort*>) and so on.  These methods generally come in
two flavors: a string version and a more structured *typed* version. The reason both flavors exist
is because the string versions are convenient and simple to use for small programs, whereas the
typed versions are typically more maintainable in larger programs.

Consider these two ways of doing a <xref:Deephaven.OpenAPI.Client.IQueryTable.Where*>, using literal strings versus using the "fluent" syntax.

```c#
var table = workerSession.QueryScope.HistoricalTable(
    "LearnDeephaven", "EODTrades");
var filtered1 = table.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`");

var (importDate, ticker) =
    table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
var filtered2 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");

PrintUtils.PrintTableData(filtered1);
PrintUtils.PrintTableData(filtered2);
```

The advantage of the `filtered1` query is that it is simple and compact.  However the advantage of
`filtered2` query is that it is able to do much more error checking at compile time. Consider the
following query syntax errors:
```c#
// typo in Ticker
var filtered1 = table.Where(
    "ImportDate == `2017-11-01` && Thicker == `AAPL`");
// nonsensical string multiplication
var filtered1 = table.Where(
    "ImportDate == `2017-11-01` && Ticker * 12 == `AAPL`");
// extra closing parenthesis
var filtered1 = table.Where(
    "(ImportDate == `2017-11-01`) && (Ticker == `AAPL`))");
```

Because the code is using the literal string syntax, these errors would not be caught until the
server attempted to parse and execute them. However, none of the corresponding fluent versions
will even *compile*!
```C#
var (importDate, ticker) =
    table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
// typo in Ticker
var filtered2 = table.Where(
    importDate == "2017-11-01" && thicker == "AAPL");
// nonsensical string multiplication
var filtered2 = table.Where(
    importDate == "2017-11-01" && ticker * 12 == "AAPL");
// extra closing parenthesis
var filtered2 = table.Where(
    (importDate == "2017-11-01") && (ticker * 12) == "AAPL"));
```

## How the fluent syntax works

The fluent syntax uses certain C# types along with operator overloading to build up an abstract
syntax tree of your expression on the client side. Then, library methods pass that tree to the
server to be executed. Because the fluent syntax is built on top of C# syntax, it needs to be legal
according to the rules of C#. One advantage of following C# syntax is that many potential errors are
caught at compile time, or even sooner, e.g. by the programmer's IDE.

Consider the following code fragment:
```C#
var (a, b, c, d) =
    table.GetColumns<NumCol, NumCol, NumCol, NumCol>("A", "B", "C", "D");
var filtered = table.Where(a + b + c <= d);
```

The transformation of the expression into an abstract syntax tree is done automatically by the
compiler. Basically, infix operators like `+` and `<=` are transformed into method calls, and
certain implicit type conversions are performed. Below is a sketch of the equivalent code after the
infix operators are transformed to method calls:

```c#
NumericExpression temp1 = NumericExpression.operator+(a, b);
NumericExpression temp2 = NumericExpression.operator+(temp1, c);
BooleanExpression temp3 = NumericExpression.operator<=(temp2, d);
IQueryTable filtered = table.Where(temp3);
```

## Building expressions with the fluent syntax

The fluent syntax is designed to capture the kinds of "natural" expressions one would write in
a programming language. Rather than formally describing the syntax here, we instead provide an
informal description.

There are basically four kinds of expressions in the system:
<xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>,
<xref:Deephaven.OpenAPI.Client.Fluent.StringExpression>,
<xref:Deephaven.OpenAPI.Client.Fluent.DateTimeExpression>, and
<xref:Deephaven.OpenAPI.Client.Fluent.BooleanExpression>.
These model the four types of expressions we want to represent in the system.

In typical usage, client programs do not explicitly declare variables of these types. Instead,
these objects are created as anonymous temporaries (as the intermediate results of overloaded
operators) which are then consumed by other operators or by Deephaven methods like
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.Select*> or
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.Where*>.

### Local vs Remote Evaluation

Because the fluent syntax interoperates with ordinary C# expression syntax, it might not be readily
apparent which parts of a complicated C# expression are executed locally on the client machine,
and which parts are participating in an expression tree to be evaluated on the server. Generally,
the rules are:

#### Evaluated locally

* Numeric literals
* Variables
* Method calls
* Unary and binary operators involving the above

#### Evaluated at the server

* Column terminals
* Local values implicitly converted into Fluent values
* Unary operators, binary operators, and certain special methods involving Fluent expressions

Note that both of these definitions are intentionally recursive in nature. Also note that when
one of the arguments to a binary operator is a Fluent expression, the other argument will be
implicitly converted to a Fluent expression.

Consider the following examples:
```C#
var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
var (importDate, ticker, close) =
    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");

var x = 1;

int myFunc(int arg)
{
    return arg + 10;
}

// Equivalent Deephaven Code Studio expression is "Result = 100 + Close"
var t1a = t0.Select((100 + close).As("Result"));
// Equivalent Deephaven Code Studio expression is "Result = 300 + Close"
var t2a = t0.Select((100 + 200 + close).As("Result"));
// Equivalent Deephaven Code Studio expression is "Result = 101 + Close"
var t3a = t0.Select((100 + x + close).As("Result"));
// Equivalent Deephaven Code Studio expression is "Result = 111 + Close"
var t4a = t0.Select((100 + myFunc(x) + close).As("Result"));
```

A binary operator with at least one <xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>
yields a <xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>. Because binary operators like
`+` have left-to-right associativity, mathematically equivalent but differently-ordered expressions
might get sent to the server as a different tree:

```C#
// Equivalent Deephaven Code Studio expression is "Result = Close + 100"
var t1b = t0.Select((close + 100).As("Result"));
// Equivalent Deephaven Code Studio expression is "Result = (Close + 100) + 200"
var t2b = t0.Select((close + 100 + 200).As("Result"));
// Equivalent Deephaven Code Studio expression is "Result = (Close + 100) + 1"
var t3b = t0.Select((close + 100 + x).As("Result"));
// Equivalent Deephaven Code Studio expression is "Result = (Close + 100) + 11"
var t4b = t0.Select((close + 100 + myFunc(x)).As("Result"));
```

Note that the library is does *not* collapse `(Close + 100) + 11` into the mathematically-equivalent
`(Close + 111)`.  This difference is largely of academic interest, because the final result is the
same due to the commutative property of addition. It would probably matter only in cases of numeric
over/underflow.

### Building Fluent Expressions

In more advanced use cases, users may want to write methods that derive fluent expressions from
other fluent expressions. Some programming languages call such methods "combinators".  In
this simple example we write an `add5` function that yields the fluent expression `e + 5` for
whatever expression `e` is passed into it:

```C#

NumericExpression add5(NumericExpression e)
{
    return e + 5;
}

// Equivalent Deephaven Code Studio expression is "Result = (Close * Volume) + 5"
var t1 = t0.Select(add5(close * volume).As("Result"));
```

#### NumericExpression

<xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>s are either `Numeric terminals`
or the result of an operator applied to some combination of `Numeric terminals` and
<xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>s.

`Numeric terminals` are:
* C# numeric literals of various primitive types such as `3` and `-8.2`
* Client-side numeric variables such as `int x` or `double x`
* Client-side numeric expressions such as `x * 2 + 5`
* Numeric columns, which are typically obtained from a call like <xref:Deephaven.OpenAPI.Client.IQueryTable_GetColumnExtensions.GetColumn*>

The operators are the the usual unary arithmetic operators `+`, `-`, `~`, and
the usual binary operators `+`, `-`, `*`, `/`, `%`, `&`, `|`, `^`.

In this example, the table `t1` contains two columns: the `Ticker` column and a `Result`
columns which holds the product `Price * Volume + 12`. Notice that in a
<xref:Deephaven.OpenAPI.Client.IQueryTable.Select*> statement, when
we are creating a new column that is the result of a calculation, we need to give that new column
a name (using the <xref:Deephaven.OpenAPI.Client.Fluent.Expression_Extensions.As*> operator).
In general, the fluent syntax `expr`.<xref:Deephaven.OpenAPI.Client.Fluent.Expression_Extensions.As*>`("X")`
corresponds to Deephaven Code Studio expression `X = expr`.

```C#
var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven",
    "EODTrades");
var (importDate, ticker, close, volume) =
    table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker",
        "Close", "Volume");
var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
var t1 = t0.Select(ticker, (close * volume).As("Result"));
// string literal equivalent
var t1_literal = t0.Select("Ticker", "Result = Close * Volume");
PrintUtils.PrintTableData(t1);
PrintUtils.PrintTableData(t1_literal);
```

### StringExpression

<xref:Deephaven.OpenAPI.Client.Fluent.StringExpression>s are either `String terminals`
or the result of the `+` operator applied to some combination of `String terminals` and
<xref:Deephaven.OpenAPI.Client.Fluent.StringExpression>s.

`String terminals` are:
* C# numeric literals like `"hello"`.
* Client-side string variables such as `string x`.
* Client-side string expressions such as `x + "QQQ"`
* String columns, which are typically obtained from a call like <xref:Deephaven.OpenAPI.Client.IQueryTable_GetColumnExtensions.GetColumn*>

Example:
```c#
var t2 = t0.Select(ticker, (ticker + "XYZ").As("Result"));
var t2_literal = t0.Select("Ticker", "Result = Ticker + `XYZ`");
```

<xref:Deephaven.OpenAPI.Client.Fluent.StringExpression> provides four additional methods that work on
<xref:Deephaven.OpenAPI.Client.Fluent.StringExpression>s.
These operations have the semantics described in the Deephaven documentation, and they yield
<xref:Deephaven.OpenAPI.Client.Fluent.BooleanExpression>s (described in a later section). For example:

```C#
var t1 = t0.Where(ticker.StartsWith("AA"));
var t1_literal = t0.Where("ticker.startsWith(`AA`)");
var t2 = t0.Where(ticker.Matches(".*P.*"));
var t2_literal = t0.Where("ticker.matches(`.*P.*`)");
```
### DateTimeExpression

`DBDateTime terminals` are:
* C# string literals, variables or string expressions in Deephaven <xref:Deephaven.OpenAPI.Client.Data.DBDateTime> format,
  e.g. `"2020-03-01T09:45:00.123456 NY"`.
* Client-side variables/expressions of type <xref:Deephaven.OpenAPI.Client.Data.DBDateTime>

<xref:Deephaven.OpenAPI.Client.Data.DBDateTime> is the standard Deephaven Date/Time type,
representing nanoseconds since January 1, 1970 UTC.

Note: the Deephaven <xref:Deephaven.OpenAPI.Client.Data.DBDateTime> type has a higher resolution (1
ns) than the .NET `System.DateTime` type (100 ns). Care should be taken when when converting from a
<xref:Deephaven.OpenAPI.Client.Data.DBDateTime> to a `System.DateTime` type, as precision can be lost.

### BooleanExpression

<xref:Deephaven.OpenAPI.Client.Fluent.BooleanExpression>s can be used to represent expressions involving boolean-valued columns (e.g.
`!boolCol1 || boolCol2`) but more commonly, they are used to represent the result of
relational operators applied to other expression types. <xref:Deephaven.OpenAPI.Client.Fluent.BooleanExpression>s support the unary
operator `!`, as well as the binary operators `&&` and `||` and their cousins `&` and `|`.

Note that the shortcutting operators `&&` and `||` do not exhibit their usual shortcutting behavior
when used with Deephaven fluent expressions. Because the value of either side of the expression isn't
knowable until it is evaluated at the server, it is not possible (nor even particularly meaningful)
to do shortcutting on the client.
As a consequence of this, `&&` is a synonym for the (non-shortcutting) boolean `&` operator; likewise
`||` is a synonym for the non-shortcutting boolean `|` operator.

For example, in `t1 = t0.Where(col0 < 5 && col1 > 12)` we would send the whole expression to
the server for evaluation. There would be no attempt to first determine the "truth" of
`col0 < 5` (a concept that doesn't even make much sense anyway in the context of a full column of
data) in order to try shortcut the evaluation of `col1 > 12`.

This example creates two boolean-valued columns and does simplistic filtering on them:
```c#
var empty = workerSession.QueryScope.EmptyTable(5, new string[0], new string[0]);
var t = empty.Update(true.As("A"), false.As("B"));
// Deephaven Code Studio equivalent
var t_literal = empty.Update("A = true", "B = false");
var (a, b) = t.GetColumns<BoolCol, BoolCol>("A", "B");
var t2 = t.Where(a);
var t3 = t.Where(a && b);
PrintUtils.PrintTableData(t2);
PrintUtils.PrintTableData(t3);
```

More commonly, `BooleanExpressions` are created as the result of relational operators on other
expressions. For example we might say

```c#
var aValues = new IntColumnData(new[] {10, 20, 30});
var sValues = new StringColumnData(new[] {"x", "y", "z"});
var temp = workerSession.QueryScope.TempTable(new[]
{
    new ColumnDataHolder("A", aValues),
    new ColumnDataHolder("S", sValues)
});
var a = temp.GetColumn<NumCol>("A");
var result = temp.Where(a > 15);
PrintUtils.PrintTableData(result);
```

Here `a > 15` applies the `>` operator to two
<xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>s, yielding a
<xref:Deephaven.OpenAPI.Client.Fluent.BooleanExpression> suitable for passing to the
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.Where*> clause and being evaluated on the
server. The library supports the usual relational operators (`<`, `<=`, `==`, `>=`, `>`, `!=`) on
<xref:Deephaven.OpenAPI.Client.Fluent.NumericExpression>,
<xref:Deephaven.OpenAPI.Client.Fluent.StringExpression>, and
<xref:Deephaven.OpenAPI.Client.Fluent.DateTimeExpression>; meanwhile
<xref:Deephaven.OpenAPI.Client.Fluent.BooleanExpression> itself supports only `==` and `!=`.

### Column Terminals

A Column Terminal is used to represent a database column symbolically, so it can be used in a
fluent invocation such as `t`.<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.Where*>`(a > 5)`.
To do this, the program needs to know the name of
the database column (in this example, "A") as well as its type (in this example, <xref:Deephaven.OpenAPI.Client.Fluent.NumCol>):
```c#
var a = temp.GetColumn<NumCol>("A");
```

The Column Terminal types are:
* <xref:Deephaven.OpenAPI.Client.Fluent.NumCol>
* <xref:Deephaven.OpenAPI.Client.Fluent.StrCol>
* <xref:Deephaven.OpenAPI.Client.Fluent.DateTimeCol>
* <xref:Deephaven.OpenAPI.Client.Fluent.BoolCol>

Note that the single fluent type <xref:Deephaven.OpenAPI.Client.Fluent.NumCol> stands in for all the
numeric types (`short`, `int`, `double`, and so on). This does *not* mean that the server represents
all these types as the same thing, or that there is some kind of loss of precision involved. Rather
it is simply a reflection of the fact that the numeric types generally interoperate with each other
and support all the same operators; from the point of view of the fluent layer, when building an
abstract syntax tree for an expression like `x + y` for evaluation at the server, it's not necessary
to know the exact types of `x` and `y` at this point, other than knowing that they are numbers.

The syntax for creating a single Column Terminal is

```c#
var col = table.GetColumn<ColumnType>(name);
```

where `ColumnType` is one of
(<xref:Deephaven.OpenAPI.Client.Fluent.NumCol>,
<xref:Deephaven.OpenAPI.Client.Fluent.StrCol>,
<xref:Deephaven.OpenAPI.Client.Fluent.DateTimeCol>, or
<xref:Deephaven.OpenAPI.Client.Fluent.BoolCol>)

and `name` is the name of the column. To conveniently bind more than one column at a time, the program can use one of
the <xref:Deephaven.OpenAPI.Client.IQueryTable_GetColumnExtensions.GetColumns*>
overloads. For example this statement binds three columns at once:
```c#
var (importDate, ticker, close) =
    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
```

### SelectColumns

An <xref:Deephaven.OpenAPI.Client.Fluent.ISelectColumn> is an object suitable to be passed to a
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.Select*> statement (or
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.Update*>,
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.View*>, or
<xref:Deephaven.OpenAPI.Client.IQueryTable_FluentExtensions.UpdateView*>).
It either needs to either refer to an already-existing column,
or it is an expression bound to a column name, which will cause a new column to be created. Examples:
```c#
// "close" is already a column, so we can use it directly
var t1 = t0.Select(close);
// "100 + close" is an expression; to turn it into a SelectColumn
// we need to bind it to a new column name with the "As" method.
var t2 = t0.Select((100 + close).As("Result"));
// The above would be expressed in the Deephaven Code Studio as:
var t2_literal = t0.select("Result = 100 + Close")
```

## What's Next

In the next chapter, we cover asynchronous [Events](./events.md).
