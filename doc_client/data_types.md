# Open API Data Types

The mapping between Deephaven types and Open API types is described below. In a couple of cases,
the closest .NET type is not a perfect match, due to precision or range issues. In those cases
we provide our own preferred type and support the .NET type as a fallback. Also,
certain unsupported types can be accessed as strings.

All Deephaven column types have a special value that represents null. For most primitive
types, this value is taken from the range of possible values, which reduces the range of
representable values. This is done so that the server can store nulls efficiently. The class
<xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants> defines constants containing these special
values.

The mappings between server and .NET types are shown below:

## Type Mappings
This table shows all supported Deephaven types and corresponding .NET type.

| Deephaven Type | Preferred Type | Alt Type | Null Value |
| -------------- | --------- | -------------- | -------- |
| Boolean | `Nullable<bool>` | | null |
| byte | sbyte | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_BYTE> |
| char | char | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_CHAR> |
| short | short | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_SHORT> |
| int | int | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_INT> |
| long | long | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_LONG> |
| float | float | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_FLOAT> |
| double | double | | <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_DOUBLE> |
| String | string | | null |
| com.illumon.iris. db.tables.utils. DBDateTime | Deephaven.OpenAPI. Client.Data. DBDateTime | System.DateTime | <xref:Deephaven.OpenAPI.Client.Data.DBDateTime.Nanos> == <xref:Deephaven.OpenAPI.Client.Data.DeephavenConstants.NULL_LONG> |
| java.math. BigDecimal | <xref:Deephaven.OpenAPI.Client.Data.DHDecimal> | decimal | null |
| java.math. BigInteger | System.Numerics. BigInteger | | null |


## Column Data
Data may arrive at the client via either an
<xref:Deephaven.OpenAPI.Client.ITableSnapshot> or
<xref:Deephaven.OpenAPI.Client.ITableUpdate> object
(as the result of
<xref:Deephaven.OpenAPI.Client.IQueryTable.Snapshot*> or
<xref:Deephaven.OpenAPI.Client.IQueryTable_TaskExtensions.Subscribe*> operations, respectively).
In either case, data is accessible in a "column-major" format, encapsulated by classes implementing the
<xref:Deephaven.OpenAPI.Client.Data.IColumnData> interface.
This interface is quite similar to result set abstractions found in ADO/JDBC-type APIs. The base class provides a number of getter
methods, but only the ones relevant for a given type are implemented; the rest throw `NotSupportedException`.
However, <xref:Deephaven.OpenAPI.Client.Data.IColumnData.GetString*> and
<xref:Deephaven.OpenAPI.Client.Data.IColumnData.GetObject*> are implemented by all the types.
For example, the only numeric getters implemented by
<xref:Deephaven.OpenAPI.Client.Data.IntColumnData>
are
<xref:Deephaven.OpenAPI.Client.Data.IntColumnData.GetInt32*> and
<xref:Deephaven.OpenAPI.Client.Data.IntColumnData.GetInt64*>.

Typically, <xref:Deephaven.OpenAPI.Client.Data.IColumnData.IsNull*> method is used to detect nulls,
rather than using the `NULL_xxx` constants directly.

Generally "non-lossy" (i.e. widening) getters are implemented as well as a few potentially
lossy/overflow-able getters that are convenient in .NET (i.e. `decimal` may be more convenient
than <xref:Deephaven.OpenAPI.Client.Data.DHDecimal> and its more limited range may not matter for
some clients).

A function illustrating how to iterate a set of column data is below.

```C#
var table = workerSession.QueryScope.HistoricalTable("LearnDeephaven", "EODTrades");
var (importDate, ticker) =
    table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
var ten = table.Where(importDate == "2017-11-01" && ticker == "AAPL").Head(10);
var tableData = ten.GetTableData();
var columnData = tableData.ColumnData;
for (var row = 0; row < tableData.Rows; ++row)
{
    var separator = "";
    foreach (var col in columnData)
    {
        var o = col.GetObject(row);
        var humanReadable = o != null ? o.ToString() : "(null)";
        Console.Write($"{separator}{humanReadable}");
        separator = ",";
    }
    Console.WriteLine();
}
```
