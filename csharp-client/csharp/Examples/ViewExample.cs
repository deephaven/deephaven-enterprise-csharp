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
    public static class ViewExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                // literal strings
                var t1 = table.LastBy("Ticker").View("Ticker", "Close", "Volume");
                PrintUtils.PrintTableData(t1);

                // Symbolically

                var (ticker, close, volume) = table.GetColumns<StrCol, NumCol, NumCol>("Ticker", "Close", "Volume");
                var t2 = table.LastBy(ticker).View(ticker, close, volume);
                PrintUtils.PrintTableData(t2);
            }
        }
    }
}
