/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client;

namespace Examples
{
    public static class TimeTableExample
    {
        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var newScope = scope.NewScope())
            {
                var tt = newScope.TimeTable(DateTime.Now, TimeSpan.FromSeconds(3))
                    .UpdateView("Blah=`jojojo`", "Moo=`kosak`")
                    .Tail(10)
                    .Preemptive(100);
                tt.OnTableUpdate += PrintUtils.ShowTableUpdate;
                tt.SubscribeAll("Blah");

                PrintUtils.PrintTableData(tt);

                Console.Write("Hit enter to stop subscribe and quit...");
                Console.ReadLine();

                tt.Unsubscribe();
            }
        }
    }
}
