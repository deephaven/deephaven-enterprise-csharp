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
    public static class MergeTablesExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                var t2 = table.Where(importDate == "2017-11-01");

                // Run a merge by fetching two tables and them merging them
                var aaplTable = t2.Where(ticker == "AAPL").Tail(10);
                var zngaTable = t2.Where(ticker == "ZNGA").Tail(10);

                var merged = aaplTable.Merge(zngaTable);
                PrintUtils.PrintTableData(merged);
            }
        }
    }
}
