/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading.Tasks;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class UngroupExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                table = table.Where(importDate == "2017-11-01");

                var byTable = table.Where(ticker == "AAPL").View("Ticker", "Close").By("Ticker");

                var ungrouped = byTable.Ungroup("Close");
                PrintUtils.PrintTableData(ungrouped);

                var ungrouped2 = byTable.Ungroup();
                PrintUtils.PrintTableData(ungrouped2);
            }
        }
    }
}
