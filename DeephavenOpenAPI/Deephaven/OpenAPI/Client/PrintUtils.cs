/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Client
{
    public static class PrintUtils
    {
        public static void PrintTableData(IQueryTable queryTable)
        {
            var columns = queryTable.GetColumns();
            PrintTableData(queryTable, columns);
        }

        public static void PrintTableData(IQueryTable queryTable, params IColumn[] columns)
        {
            var colsAsStrings = columns.Select(c => c.Name).ToArray();
            PrintTableData(queryTable, colsAsStrings);
        }

        public static void PrintTableData(IQueryTable queryTable, params string[] columns)
        {
            PrintHeaders(columns);
            foreach (var td in StreamTableData(queryTable, columns))
            {
                PrintChunk(columns, false, td.IncludedRows.Count, td.ColumnData);
            }
        }

        public static void PrintChunk(IEnumerable<IColumn> columns, bool wantHeaders, long rows,
            IEnumerable<IColumnData> columnData)
        {
            PrintChunk(columns.Select(c => c.Name), wantHeaders, rows, columnData);
        }


        public static void PrintChunk(IEnumerable<string> columns, bool wantHeaders, long rows,
            IEnumerable<IColumnData> columnData)
        {
            if (wantHeaders)
            {
                PrintHeaders(columns);
            }

            // reify
            var colData = columnData.ToArray();

            for (var row = 0; row < rows; row++)
            {
                Console.WriteLine(colData.MakeSeparatedList("\t",
                    cd => cd == null ? "" : cd.GetString(row)));
            }
        }

        private static void PrintHeaders(IEnumerable<string> columns)
        {
            Console.WriteLine(columns.MakeSeparatedList("\t"));
        }

        /// <summary>
        /// Make an enumerable of ITableData. We take pains to try to keep a next query pending
        /// while we are returning data for the current one.
        /// </summary>
        private static IEnumerable<ITableData> StreamTableData(IQueryTable queryTableArg,
            string[] cols)
        {
            const long chunkSize = 100_000;

            using (queryTableArg.NewScope(out var qt))
            {
                var empty = qt.Scope.EmptyTable(0, new string[0], new string[0]);
                qt = empty.Snapshot(qt);

                var size = qt.TableState.Resolve()._tableDefinition.Size;
                Task<ITableData> lastTask = null;
                for (var begin = 0L; begin < size; begin += chunkSize)
                {
                    var end = Math.Min(begin + chunkSize, size);
                    var thisTask = qt.GetTableDataTask(begin, end - 1, cols);
                    if (lastTask != null)
                    {
                        yield return lastTask.Result;
                    }

                    lastTask = thisTask;
                }

                if (lastTask != null)
                {
                    yield return lastTask.Result;
                }
            }
        }

        public static void ShowTableUpdate(IQueryTable table, ITableUpdate tableUpdate)
        {
            // TODO(kosak)
            Console.WriteLine($"ShowTableUpdate: got table update {tableUpdate}:");
        }
    }
}
