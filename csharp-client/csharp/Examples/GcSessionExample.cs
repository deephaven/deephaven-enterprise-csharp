/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven;
using Deephaven.OpenAPI.Client;

namespace Examples
{
    public static class GcSessionExample
    {
        public static void Run(IOpenApiClient client, IQueryScope _)
        {
            for (var i = 0; i < 10; ++i)
            {
                MakeWorker(client, i);
                GC.Collect();
            }
        }

        private static void MakeWorker(IOpenApiClient client, int i)
        {
            Console.WriteLine($"Starting worker {i}...");
            var workerOptions = new WorkerOptions("Default");

            // Deliberately not using "using"... will we get the finalizer?
            var workerSession = client.StartWorker(workerOptions);
            var table = workerSession.QueryScope.HistoricalTable(DemoConstants.HistoricalNamespace,
                DemoConstants.HistoricalTable);
            var t1 = table.Where("ImportDate == `2017-11-01` && Ticker == `AAPL`").Head(3);
            PrintUtils.PrintTableData(t1);
        }
    }
}
