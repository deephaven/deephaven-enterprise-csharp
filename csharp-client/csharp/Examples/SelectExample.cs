/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class SelectExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                Test0(newScope);
                var table = scope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                Test1(table);
                Test1a(table);
                Test2(table);
                Test3(table);
                Test4(table);
                Test5(table);
            }
        }

        private static void Test0(IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var intData = new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
                var doubleData = new[] {0.0, 1.1, 2.2, 3.3, 4.4, 5.5, 6.6, 7.7, 8.8, 9.9};
                var t = newScope.TempTable(new[]
                {
                    new ColumnDataHolder("IntValue", new IntColumnData(intData)),
                    new ColumnDataHolder("DoubleValue", new DoubleColumnData(doubleData))
                });

                var t2 = t.Update("Q2 = IntValue * 100");
                PrintUtils.PrintTableData(t2);
                var t3 = t2.Update("Q3 = Q2 + 10");
                PrintUtils.PrintTableData(t3);
                var q2 = t3.GetColumn<NumCol>("Q2");
                var t4 = t3.Update((q2 + 100).As("Q4"));
                PrintUtils.PrintTableData(t4);
            }
        }

        // Simple Where
        private static void Test1(IQueryTable table)
        {
            using (table.NewScope(out var t))
            {
                // String literal
                var t1 = t.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`");
                PrintUtils.PrintTableData(t1);

                // Symbolically
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                var t2 = t.Where(importDate == "2017-11-01" && ticker == "AAPL");
                PrintUtils.PrintTableData(t2);
            }
        }

        // Simple Where with syntax error
        private static void Test1a(IQueryTable table)
        {
            using (table.NewScope(out var t))
            {
                try
                {
                    // String literal
                    var t1 = t.Where(")))))");
                    PrintUtils.PrintTableData(t1);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Caught *expected* exception {e.Message}");
                    // Expected
                    return;
                }
            }

            throw new Exception("Expected failure.");
        }


        // Select a few columns
        private static void Test2(IQueryTable table)
        {
            using (table.NewScope(out var t))
            {
                var t1 = t.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`")
                    .Select("Ticker", "Close", "Volume");
                PrintUtils.PrintTableData(t1);

                // Symbolically
                var (importDate, ticker, close, volume) =
                    table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker", "Close",
                        "Volume");
                var t2 = t.Where(importDate == "2017-11-01" && ticker == "AAPL")
                    .Select(ticker, close, volume);
                PrintUtils.PrintTableData(t2);
            }
        }

        // LastBy + Select
        private static void Test3(IQueryTable table)
        {
            using (table.NewScope(out var t))
            {
                var t1 = t.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`").LastBy("Ticker")
                    .Select("Ticker", "Close", "Volume");
                PrintUtils.PrintTableData(t1);

                // Symbolically
                var (importDate, ticker, close, volume) =
                    table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker", "Close",
                        "Volume");
                var t2 = t.Where(importDate == "2017-11-01" && ticker == "AAPL").LastBy(ticker)
                    .Select(ticker, close, volume);
                PrintUtils.PrintTableData(t2);
            }
        }

        // Formula in the where clause
        private static void Test4(IQueryTable table)
        {
            using (table.NewScope(out var t))
            {
                var t1 = t.Where("ImportDate == `2017-11-01` && Ticker == `AAPL` && Volume % 10 == Volume % 100")
                    .Select("Ticker", "Volume");
                PrintUtils.PrintTableData(t1);

                // Symbolically
                var (importDate, ticker, volume) =
                    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Volume");
                var t2 = t.Where(importDate == "2017-11-01" && ticker == "AAPL" && volume % 10 == volume % 100)
                    .Select(ticker, volume);
                PrintUtils.PrintTableData(t2);
            }
        }

        // New columns
        private static void Test5(IQueryTable table)
        {
            using (table.NewScope(out var t))
            {
                var t1 = t.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`").Select("Volume");
                PrintUtils.PrintTableData(t1);

                // A formula expression
                var t2 = t.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`")
                    .Select("MV1 = Volume * Close", "MV12 = Volume + 12");
                PrintUtils.PrintTableData(t2);

                // Symbolically
                var (importDate, ticker, close, volume) =
                    table.GetColumns<StrCol, StrCol, NumCol, NumCol>("ImportDate", "Ticker", "Close",
                        "Volume");
                var t3 = t.Where(importDate == "2017-11-01" && ticker == "AAPL")
                    .Select(volume);
                PrintUtils.PrintTableData(t3);

                // A formula in the where clause
                var t4 = t.Where(importDate == "2017-11-01" && ticker == "AAPL" && close == volume + 12)
                    .Select(volume);
                PrintUtils.PrintTableData(t4);

                // Epression.As("New Column Name")
                var t5 = t.Where(importDate == "2017-11-01" && ticker == "AAPL")
                    .Select((volume * close).As("MV1"), (volume + 12).As("MV_by_12"));
                PrintUtils.PrintTableData(t5);
            }
        }
    }
}
