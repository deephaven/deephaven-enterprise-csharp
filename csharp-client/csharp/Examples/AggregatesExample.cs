/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Fluent;
using static Deephaven.OpenAPI.Client.DeephavenImports;

namespace Examples
{
    public static class AggregatesExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker, close) =
                    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
                table = table.Where(importDate == "2017-11-01");

                var zngaTable = table.Where(ticker == "ZNGA");

                // For fun, make a sub-scope using this convenience method that also gives us a table inside the scope
                using (zngaTable.NewScope(out var scopedZnga))
                {
                    PrintUtils.PrintTableData(scopedZnga.HeadBy(5, ticker));
                    PrintUtils.PrintTableData(scopedZnga.TailBy(5, ticker));
                    // the server tables created by "HeadBy" and "TailBy" will be disposed on the way out of this scope
                }

                var aggTable = zngaTable.View(close)
                    .By(AggregateCombo.Create(
                        Aggregate.Avg("AvgClose=Close"),
                        Aggregate.Sum("SumClose=Close"),
                        Aggregate.Min("MinClose=Close"),
                        Aggregate.Max("MaxClose=Close"),
                        Aggregate.Count("Count")));

                var aggTable2 = zngaTable.View(close)
                    .By(AggCombo(
                        AggAvg(close.As("AvgClose")),
                        AggSum(close.As("SumClose")),
                        AggMin(close.As("MinClose")),
                        AggMax(close.As("MaxClose")),
                        AggCount("Count")));
                PrintUtils.PrintTableData(aggTable);
                PrintUtils.PrintTableData(aggTable2);
            }
        }
    }
}
