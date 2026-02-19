/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class LastByExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var table = scope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable))
            {
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                PrintUtils.PrintTableData(table.Where(importDate == "2017-11-01").LastBy(ticker));
            }
        }
    }
}
