/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven;
using Deephaven.OpenAPI.Client;

namespace Examples
{
    public static class DropColumnsExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable).Head(100);
                var t2 = table.DropColumns("Open", "Volume");
                PrintUtils.PrintTableData(t2);
            }
        }
    }
}
