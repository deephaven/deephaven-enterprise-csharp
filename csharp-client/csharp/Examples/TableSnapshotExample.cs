/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class TableSnapshotExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker, close, eodTimestamp) =
                    table.GetColumns<StrCol, StrCol, NumCol, DateTimeCol>("ImportDate", "Ticker", "Close", "EODTimestamp");
                var t2 = table.Where(importDate == "2017-11-01").Where(ticker == "AAPL" && close <= 120.0);

                var columns = new IColumn[] {eodTimestamp, close};
                PrintUtils.PrintTableData(t2, columns);
            }
        }
    }
}
