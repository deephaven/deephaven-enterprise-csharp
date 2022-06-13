/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class FilterExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var t1 = table.Where(
                    "ImportDate == `2017-11-01` && Ticker == `AAPL` && (Close <= 120.0 || isNull(Close))");
                PrintUtils.PrintTableData(t1);

                var (importDate, ticker, close) =
                    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Close");
                var t2 = table.Where(importDate == "2017-11-01" && ticker == "AAPL" &&
                                     (close <= 120.0 || close.IsNull()));
                PrintUtils.PrintTableData(t2);

                // TODO(kosak)
                // PrintUtils.PrintTableData(table.Where("2017-11-01" == importDate && Condition.Search("AAPL")));
            }
        }
    }
}
