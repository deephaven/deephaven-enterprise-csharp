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
    public static class SnapshotExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var table = newScope.HistoricalTable(DemoConstants.HistoricalNamespace, DemoConstants.HistoricalTable);
                var (importDate, ticker) = table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
                table = table.Where(importDate == "2017-11-01");

                var aaplQuery = table.Where(ticker == "AAPL");

                // sample once (no time table)
                var fz = aaplQuery.Freeze();
                PrintUtils.PrintChunk(fz.GetColumns(), true, 3, fz.GetTableData().ColumnData);

                // create a time table query
                var timeTable = newScope.TimeTable(DateTime.UtcNow, TimeSpan.FromSeconds(1));
                PrintUtils.PrintTableData(timeTable);

                // sample the appl query using the time table
                var samplingTable = timeTable.Snapshot(aaplQuery).Preemptive(100);

                samplingTable.OnTableUpdate += PrintUtils.ShowTableUpdate;
                samplingTable.SubscribeAll();

                Console.WriteLine("Hit enter to stop subscribe and quit...");
                Console.ReadLine();

                samplingTable.Unsubscribe();
            }
        }

    }
}
