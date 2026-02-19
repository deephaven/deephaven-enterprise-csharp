/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class HeadAndTailExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var importDate = table.GetColumn<StrCol>("ImportDate");

                Console.WriteLine("==== Head(100) ====");
                PrintUtils.PrintTableData(table.Where(importDate == "2017-11-01").Head(100));

                Console.WriteLine("==== Tail(100) ====");
                PrintUtils.PrintTableData(table.Where(importDate == "2017-11-01").Tail(100));
            }
        }
    }
}
