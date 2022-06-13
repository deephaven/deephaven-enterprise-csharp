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
    public static class JoinExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker, volume) =
                    table.GetColumns<StrCol, StrCol, NumCol>("ImportDate", "Ticker", "Volume");
                table = table.Where(importDate == "2017-11-01");

                var lastClose = table.LastBy(ticker);
                var adv = table.View(ticker, volume).AvgBy(ticker);

                // do a join on a resolved RHS
                var joined = lastClose.NaturalJoin(adv, new[] {ticker}, new[] {volume.As("ADV")});
                PrintUtils.PrintTableData(joined);
            }
        }
    }
}
