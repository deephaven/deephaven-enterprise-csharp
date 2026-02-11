# Table Operations

Many of the most common table operations available in Deephaven Code Studio are available in the Open API.
This includes filters, sorts, custom columns, joins and aggregates. For the most part these operations work in the same
way as operations of the same type in Deephaven Code Studio. There are some imitations however, which are noted here.

## Filtering

Filtering a table is accomplished via one of the
<xref:Deephaven.OpenAPI.Client.IQueryTable.Where*> entry points. Calling code can use a Deephaven Code Studio style literal string
or a [Fluent Expression](./fluent.md). Example:

```c#
using (var workerSession = client.StartWorker(workerOptions))
{
    var table = workerSession.QueryScope.HistoricalTable(
        "LearnDeephaven", "EODTrades");
    var filtered1 = table.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`");

    var (importDate, ticker) =
        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
    var filtered2 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");

    PrintUtils.PrintTableData(filtered1);
    PrintUtils.PrintTableData(filtered2);
}
```

## Sorting

Sorting is accomplished via one of the
<xref:Deephaven.OpenAPI.Client.IQueryTable.Sort*> entry points. Calling code can use a Deephaven Code Studio style literal string
or a [Fluent Expression](./fluent.md). Consider the following (inefficient) example:

```C#
var scope = workerSession.QueryScope;
var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");

var filtered1 = table
    .Where("ImportDate == `2017-11-01` && Ticker.startsWith(`K`)")
    .Select("Ticker", "Close", "Volume")
    .Sort("Volume")
    .SortDescending("Ticker");

var (importDate, ticker, close, volume) =
    table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
        "ImportDate", "Ticker", "Close", "Volume");
var filtered2 = table
    .Where(importDate == "2017-11-01" && ticker.StartsWith("K"))
    .Select(ticker, close, volume)
    .Sort(volume)
    .SortDescending(ticker);

PrintUtils.PrintTableData(filtered1);
PrintUtils.PrintTableData(filtered2);
```

As usual the example shows this both ways, first with the Deephaven Code Studio style literal syntax and then with the
[Fluent](./fluent.md) syntax.  Here we first sort the table by `Volume`, ascending, and then we
(stably) re-sort the result by `Ticker`, descending. The result is a table sorted by descending
tickers then ascending volumes. Note the standard sorting trick where we sort in sequence from
minor to major, with the most major key being last.

The system can do this operation more efficiently by allowing us to specify both parts of the sort
key at once. The situation is slightly complicated by the fact that we want one element of the key
to be sorted in the ascending direction, and the other element in the descending direction. To deal
with this, Deephaven allows us to specify a <xref:Deephaven.OpenAPI.Client.SortPair> which binds a column name
to a sorting direction (and also an optional absolute-value specification). The
[Fluent](./fluent.md) version is even more compact thanks to the extension methods
<xref:Deephaven.OpenAPI.Client.SortPair_Extensions.Ascending*> and
<xref:Deephaven.OpenAPI.Client.SortPair_Extensions.Descending*>. Example:


```C#
var scope = workerSession.QueryScope;
var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");

var filtered1 = table
    .Where("ImportDate == `2017-11-01` && Ticker.startsWith(`K`)")
    .Select("Ticker", "Close", "Volume")
    .Sort(SortPair.Ascending("Ticker"), SortPair.Descending("Volume"));

var (importDate, ticker, close, volume) =
    table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
        "ImportDate", "Ticker", "Close", "Volume");
var filtered2 = table
    .Where(importDate == "2017-11-01" && ticker.StartsWith("K"))
    .Select(ticker, close, volume)
    .Sort(ticker.Ascending(), volume.Descending());

PrintUtils.PrintTableData(filtered1);
PrintUtils.PrintTableData(filtered2);
```

Here, because we are specifying the sort key all at once in a single sort operation, we specify the
most major part of the key *first*.

## Custom Columns

<xref:Deephaven.OpenAPI.Client.IQueryTable> provides the
<xref:Deephaven.OpenAPI.Client.IQueryTable.UpdateView*>,
<xref:Deephaven.OpenAPI.Client.IQueryTable.Update*>, and
<xref:Deephaven.OpenAPI.Client.IQueryTable.LazyUpdate*> operations,
all of which produce custom columns. See the primary Deephaven documentation for details on the differences
between these operations. Note - for security reasons the expressions permitted via Open API are more limited than in
Groovy/Python.

````C#
var joined = table.UpdateView("AvgPrice=(Open+Close/2.0)");
````

## Select/View Columns
<xref:Deephaven.OpenAPI.Client.IQueryTable> provides the
<xref:Deephaven.OpenAPI.Client.IQueryTable.Select*>,
<xref:Deephaven.OpenAPI.Client.IQueryTable.Update*>,
<xref:Deephaven.OpenAPI.Client.IQueryTable.View*>, and
<xref:Deephaven.OpenAPI.Client.IQueryTable.UpdateView*> entry points. The differences between them are described
in the primary Deephaven documentation.

Calling code can use an Deephaven Code Studio style literal string or the [Fluent Syntax](./fluent.md).

```C#
var t1_literal = t0.Select("Ticker", "AvgPrice = (Open + Close) / 2.0");
var t1_fluent = t0.Select(ticker, ((open + close) / 2.0).As("AvgPrice"));
```

## Drop Columns
<xref:Deephaven.OpenAPI.Client.IQueryTable.DropColumns*> can be used to remove specified columns.

```C#
var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
var (importDate, ticker, open, close) =
    table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
        "ImportDate", "Ticker", "Open", "Close");
var t0 = table
    .Where(importDate == "2017-11-01" && ticker == "AAPL")
    .Select(ticker, open, close);
var t1_literal = t0.DropColumns("Open");
var t1_fluent = t0.DropColumns(open);
```

## Joins
Open API provides inner join, natural join, as of join, reverse as of join, exact join, and left join operations.
These operators behave in the same way as the Groovy/Python operations of the same name. Each join type takes match
columns and columns to add from the right-hand side. See the primary Deephaven documentation for details. Here we
illustrate just a few common use-cases.

### Simple Example
This contrived example uses inner join to show only the trades from 2017-11-01 where we have a phone number on file:
```C#
var scope = workerSession.QueryScope;
var tickerValues = new StringColumnData(new[] {"AAPL", "IBM"});
var phoneValues = new StringColumnData(new[] {"1-800-AAA-AAPL", "1-888-IBM-XXXX"});
var phones = scope.TempTable(new[]
{
    new ColumnDataHolder("Ticker", tickerValues),
    new ColumnDataHolder("Phone", phoneValues)
});
var (phonesTicker, phonesNumber) =
    phones.GetColumns<StrCol, StrCol>("Ticker", "Phone");

var trades = scope.HistoricalTable("LearnDeephaven", "EODTrades");
var (importDate, ticker) =
    trades.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
var filtered = trades.Where(importDate == "2017-11-01");

var result_literal = filtered.InnerJoin(
    phones, new[] {"Ticker"}, new[] {"Phone"});
var result_fluent = filtered.InnerJoin(
    phones, new[] {ticker}, new[] {phonesNumber});
PrintUtils.PrintTableData(result_literal);
PrintUtils.PrintTableData(result_fluent);
```

### Match Pairs
This example is similar to the previous, except the join column names don't match:
here, we need to match the "Ticker" table on the left with the "PhonesTicker" column on the right.

```C#
var scope = workerSession.QueryScope;
var tickerValues = new StringColumnData(new[] {"AAPL", "IBM"});
var phoneValues = new StringColumnData(new[] {"1-800-AAA-AAPL", "1-888-IBM-XXXX"});
var phones = scope.TempTable(new[]
{
    new ColumnDataHolder("PhonesTicker", tickerValues),
    new ColumnDataHolder("Phone", phoneValues)
});
var (phonesTicker, phonesNumber) =
    phones.GetColumns<StrCol, StrCol>("PhonesTicker", "Phone");

var trades = scope.HistoricalTable("LearnDeephaven", "EODTrades");
var (importDate, ticker) =
    trades.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
var filtered = trades.Where(importDate == "2017-11-01");

var result_literal = filtered.InnerJoin(phones, new[] {"Ticker=PhonesTicker"},
    new[] {"Phone"});
var result_fluent = filtered.InnerJoin(phones, new[] {ticker.MatchWith(phonesTicker)},
    new[] {phonesNumber});
PrintUtils.PrintTableData(result_literal);
PrintUtils.PrintTableData(result_fluent);
```

### Rename Output Columns
This example is also similar to the previous; the difference here is that we want to rename the output column.
```C#
...
var result_literal = filtered.InnerJoin(
    phones, new[] {"Ticker=PhonesTicker"}, new[] {"AddedPhone=Phone"});
var result_fluent = filtered.InnerJoin(
    phones, new[] {ticker.MatchWith(phonesTicker)},
        new[] {phonesNumber.As("AddedPhone")});
...
```

## Aggregates

<xref:Deephaven.OpenAPI.Client.IQueryTable>
provides a number of single operator aggregate operations
such as
<xref:Deephaven.OpenAPI.Client.IQueryTable.MinBy*>,
<xref:Deephaven.OpenAPI.Client.IQueryTable.MaxBy*>,
<xref:Deephaven.OpenAPI.Client.IQueryTable.SumBy*>, and
<xref:Deephaven.OpenAPI.Client.IQueryTable.CountBy*>,
as well as a general
<xref:Deephaven.OpenAPI.Client.AggregateCombo>
facility that will execute any number of aggregates in a single operation.

### Single Aggregate
This example sums the Volume column by Ticker.
```C#
var trades = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
var (importDate, ticker, volume) =
    trades.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Volume");
var filtered = trades.Where(importDate == "2017-11-01")
    .View(ticker, volume);
var result_literal = filtered.SumBy("Ticker");
var result_fluent = filtered.SumBy(ticker);
PrintUtils.PrintTableData(result_literal);
PrintUtils.PrintTableData(result_fluent);
```
 
### Combo Aggregate
This example generates the high and low close price, total volume and the number of days/rows by ticker.
Note that the [Fluent](./fluent.md) version of the query needs the `using static` statement because it
uses
<xref:Deephaven.OpenAPI.Client.DeephavenImports.AggMin*>,
<xref:Deephaven.OpenAPI.Client.DeephavenImports.AggMax*>,
<xref:Deephaven.OpenAPI.Client.DeephavenImports.AggSum*>, and
<xref:Deephaven.OpenAPI.Client.DeephavenImports.AggCount*>.

```C#
using static Deephaven.OpenAPI.Client.DeephavenImports;
...
var table = workerSession.QueryScope.HistoricalTable(
    DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
var (ticker, close, volume) =
    table.GetColumns<StrCol, NumCol, NumCol>(
        "Ticker", "Close", "Volume");

var aggTableLiteral = table.View("Ticker", "Close", "Volume")
    .By(AggregateCombo.Create(
            Aggregate.Min("Low=Close"),
            Aggregate.Max("High=Close"),
            Aggregate.Sum("TotalVolume=Volume"),
            Aggregate.Count("Days")),
        "Ticker");

var aggTableFluent = table.View(ticker, close, volume)
    .By(AggCombo(
            AggMin(close.As("Low")),
            AggMax(close.As("High")),
            AggSum(volume.As("TotalVolume")),
            AggCount("Days")),
        "Ticker");

PrintUtils.PrintTableData(aggTableLiteral);
PrintUtils.PrintTableData(aggTableFluent);
```

## What's Next

In the next chapter, we cover the [Open API Data Types](./data_types.md).
