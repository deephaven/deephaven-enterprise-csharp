/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class SortExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker, volume) =
                    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Volume");
                // Limit by date and ticker
                var table2 = table.Where(importDate == "2017-11-01");
                var table3 = table2.Where(ticker >= "X");
                PrintUtils.PrintTableData(table3);

                var table4 = table3.Sort(SortPair.Descending("Ticker"), SortPair.Ascending("Volume"));
                PrintUtils.PrintTableData(table4);

                // SortPair.Direction(C# column var)
                var table5 = table3.Sort(SortPair.Descending(ticker), SortPair.Ascending(volume));
                PrintUtils.PrintTableData(table5);

                // with the sort direction convenience methods on the C# column var
                var table6 = table3.Sort(ticker.Descending(), volume.Ascending());
                PrintUtils.PrintTableData(table6);
            }

            var intData0 = new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 13, 14, 15, 16 };
            var intData1 = new [] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4,  5,  5,  6,  6,  7,  7 };
            var intData2 = new [] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2,  2,  2,  3,  3,  3,  3 };
            var intData3 = new [] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1,  1,  1,  1,  1,  1,  1 };
            using (var tempTable = scope.TempTable(new[]
            {
                new ColumnDataHolder("IntValue0", new IntColumnData(intData0)),
                new ColumnDataHolder("IntValue1", new IntColumnData(intData1)),
                new ColumnDataHolder("IntValue2", new IntColumnData(intData2)),
                new ColumnDataHolder("IntValue3", new IntColumnData(intData3))
            }))
            {
                var (iv0, iv1, iv2, iv3) =
                    tempTable.GetColumns<NumCol, NumCol, NumCol, NumCol>("IntValue0", "IntValue1", "IntValue2",
                        "IntValue3");

                var test = tempTable.Sort(iv3.Descending(), iv2.Ascending());
                PrintUtils.PrintTableData(test);
            }
        }
    }
}
