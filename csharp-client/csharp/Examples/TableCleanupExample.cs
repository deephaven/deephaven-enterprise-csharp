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
    /// <summary>
    /// This example shows explicit QueryTable cleanup using the Dispose pattern. Generally QueryTables do not need to
    /// be explicitly disposed, because their TableHandles will be cleaned up when their enclosing QueryScope
    /// is disposed. However, certain applications may wish to dispose QueryTables early. This example shows how to do
    /// that.
    /// </summary>
    public static class TableCleanupExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var table = scope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable))
            {
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");

                // This example will dispose each table individually. This might be handy
                // but not necessary as you can depend on the context to clean up
                using (var t1 = table.Where(importDate == "2017-11-01"))
                using (var t2 = t1.CountBy(ticker))
                {
                    PrintUtils.PrintTableData(t2);
                }
            }
        }
    }
}
