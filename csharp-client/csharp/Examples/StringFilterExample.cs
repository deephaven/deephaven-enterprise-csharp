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
    public static class StringFilterExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                var t2 = table.Where(importDate == "2017-11-01");
                Console.WriteLine("Contains AA");
                PrintUtils.PrintTableData(t2.Where(ticker.Contains("AA")));
                Console.WriteLine("Starts With AA");
                PrintUtils.PrintTableData(t2.Where(ticker.StartsWith("AA")));
                Console.WriteLine("Ends With AA");
                PrintUtils.PrintTableData(t2.Where(ticker.EndsWith("AA")));
                Console.WriteLine("Matches ^A.*A$");
                PrintUtils.PrintTableData(t2.Where(ticker.Matches("^A.*A$")));
            }
        }
    }
}
