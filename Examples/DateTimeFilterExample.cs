/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using Deephaven.OpenAPI.Client;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Examples
{
    public static class DateTimeFilterExample
    {
        private static readonly string[] labels =
        {
            "RowA",
            "RowB",
            "RowC",
            "RowD"
        };
        private static readonly DBDateTime[] dateTimeData1 =
        {
            null,
            new DBDateTime(1970, 1, 1),
            new DBDateTime(2019, 12, 31, 23, 59, 59, 999_999_999),
            new DBDateTime(1983, 3, 1, 12, 15, 0),
        };
        private static readonly DBDateTime[] dateTimeData2 =
        {
            null,
            new DBDateTime(1983, 3, 1, 12, 15, 0),
            new DBDateTime(2019, 12, 31, 23, 59, 59, 999_999_999),
            new DBDateTime(1970, 1, 1)
        };

        public static void Run(IOpenApiClient _, IQueryScope scope)
        {
            using (var table = scope.TempTable(new []
            {
                new ColumnDataHolder("Label", new StringColumnData(labels)),
                new ColumnDataHolder("DateTimeValue1", new DBDateTimeColumnData(dateTimeData1)),
                new ColumnDataHolder("DateTimeValue2", new DBDateTimeColumnData(dateTimeData2))
            }))
            {
                var (dateTime1, dateTime2) = table.GetColumns<DateTimeCol, DateTimeCol>("DateTimeValue1", "DateTimeValue2");

                Console.WriteLine("All Rows: ");
                PrintUtils.PrintTableData(table);

                Console.WriteLine("Rows where dateTime1 < dateTime2: ");
                PrintUtils.PrintTableData(table.Where(dateTime1 < dateTime2));

                // string literal for time in UTC
                Console.WriteLine("Rows where dateTime1 < 2017-08-25T09:30:00.123456789 UTC:");
                PrintUtils.PrintTableData(table.Where(dateTime1 < "2017-08-25T09:30:00.123456789 UTC"));

                // string literal for variable in UTC
                var dateVar = new DBDateTime(2017, 8, 25, 9, 30, 00, 1234567890);
                Console.WriteLine($"Rows where dateTime1 < {dateVar}");
                PrintUtils.PrintTableData(table.Where(dateTime1 < dateVar));

                // string literal for time in NY
                Console.WriteLine("Rows where dateTime1 < 2017-08-25T09:30:00.123456789 NY:");
                PrintUtils.PrintTableData(table.Where(dateTime1 < "2017-08-25T09:30:00.123456789 NY"));

                // DBDateTime doesn't carry a time zone, so we don't (yet) have a way to specify a variable that holds
                // a date in a given time zone

                // null
                Console.WriteLine("Rows where isNull(dateTime1)");
                PrintUtils.PrintTableData(table.Where(dateTime1.IsNull()));

                // not null
                Console.WriteLine("Rows where !isNull(dateTime1)");
                PrintUtils.PrintTableData(table.Where(!dateTime1.IsNull()));
            }
        }
    }
}
