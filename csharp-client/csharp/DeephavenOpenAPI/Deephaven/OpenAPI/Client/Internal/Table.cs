/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;
using Deephaven.OpenAPI.Core.API.Util;
using Deephaven.OpenAPI.Shared.Data;
using Void = Deephaven.OpenAPI.Core.RPC.Serialization.Java.Lang.Void;
using Range = Deephaven.OpenAPI.Shared.Data.Range;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class Table
    {
        private static readonly Dictionary<string, Func<string, IColumn>> factories =
            new Dictionary<string, Func<string, IColumn>>
        {
            { "int", n => new NumCol<int>(n) },
            { "short", n => new NumCol<short>(n) },
            { "long", n => new NumCol<long>(n) },
            { "byte", n => new NumCol<sbyte>(n) },
            { "char", n => new NumCol<char>(n) },  // TODO(kosak): different column type?
            { "double", n => new NumCol<double>(n) },
            { "float", n => new NumCol<float>(n) },
            { "java.lang.Boolean", n => new BoolCol(n) },
            { "java.math.BigDecimal", n => new NumCol<decimal>(n) },
            { "java.math.BigInteger", n => new NumCol<BigInteger>(n) },
            { "com.illumon.iris.db.tables.utils.DBDateTime", n => new DateTimeCol<DateTime>(n) },
            { "java.time.LocalDate", n => new DateTimeCol<DHDate>(n) },
            { "java.time.LocalTime", n => new DateTimeCol<DHTime>(n) },
        };

        private readonly ServerContext _context;

        internal readonly InitialTableDefinition _tableDefinition;

        private TableHandle TableHandle { get; }

        private readonly Dictionary<string, IColumn> _columnMap;

        private readonly Action<DeltaUpdates> _tableUpdateHandler;
        private readonly Action<TableSnapshot> _tableSnapshotHandler;

        public Table(TableHandle tableHandle, ServerContext context,
            InitialTableDefinition tableDefinition, Action<DeltaUpdates> tableUpdateHandler,
            Action<TableSnapshot> tableSnapshotHandler)
        {
            TableHandle = tableHandle;

            _context = context;
            _tableDefinition = tableDefinition;
            _tableUpdateHandler = tableUpdateHandler;
            _tableSnapshotHandler = tableSnapshotHandler;

            var columnDefinitions = _tableDefinition.Columns;
            _columnMap = new Dictionary<string, IColumn>();
            foreach (var cd in columnDefinitions) {
                var col = factories.TryGetValue(cd.Type, out var factory) ? factory(cd.Name) : new StrCol(cd.Name);
                _columnMap.Add(col.Name, col);
            }
        }

        public IColumn[] GetColumns()
        {
            return _columnMap.Values.ToArray();
        }

        public IColumn[] GetColumns(string[] names, Type[] types)
        {
            if (names.Length != types.Length)
            {
                throw new ArgumentException($"Expected names length {names.Length} == types length {types.Length}");
            }
            var problems = new List<string>();
            var result = new IColumn[names.Length];
            for (var i = 0; i < names.Length; ++i)
            {
                var name = names[i];
                if (!_columnMap.TryGetValue(name, out var column))
                {
                    problems.Add($"Can't find column \"{name}\"");
                    continue;
                }
                var expectedType = types[i];
                var actualType = column.GetType();
                if (!expectedType.IsAssignableFrom(actualType))
                {
                    problems.Add($"Expected column \"{name}\" to be assignable to type {expectedType.Name}, found {actualType.Name}");
                    continue;
                }
                result[i] = column;
            }
            if (problems.Count != 0)
            {
                throw new Exception($"GetColumns failed: {problems.MakeCommaSeparatedList()}. Available columns are {_columnMap.Values.MakeCommaSeparatedList()}");
            }
            return result;
        }

        public Task<ITableData> GetTableData()
        {
            return GetTableData(0, long.MaxValue, null);
        }

        public Task<ITableData> GetTableData(params string[] columns)
        {
            return GetTableData(0, long.MaxValue, columns);
        }

        public Task<ITableData> GetTableData(long first, long last)
        {
            return GetTableData(first, last, null);
        }

        private BitArray GetColumnsBitArray(params string[] columns)
        {
            // create the columns bit set
            BitArray columnsBitArray = new BitArray(_tableDefinition.Columns.Length);
            if (columns == null)
            {
                columnsBitArray.SetAll(true);
            }
            else
            {
                HashSet<string> columnSet = new HashSet<string>(columns);
                for (int i = 0; i < columnsBitArray.Length; i++)
                {
                    columnsBitArray.Set(i, columnSet.Contains(_tableDefinition.Columns[i].Name));
                }
            }
            return columnsBitArray;
        }

        public Task<ITableData> GetTableData(long first, long last, string[] columns)
        {
            var rows = new RangeSet();
            rows.AddRange(new Range(first, last));
            return GetTableData(new RowRangeSet(rows), columns);
        }

        public async Task<ITableData> GetTableData(RowRangeSet rowRangeSet, string[] columns)
        {
            // create the columns bit set
            var columnsBitArray = new BitArray(_tableDefinition.Columns.Length);
            if (columns == null)
            {
                columnsBitArray.SetAll(true);
            }
            else
            {
                var columnSet = new HashSet<string>(columns);
                for (var i = 0; i < columnsBitArray.Length; i++)
                {
                    columnsBitArray.Set(i, columnSet.Contains(_tableDefinition.Columns[i].Name));
                }
            }

            var task = _context.InvokeServerTask<TableSnapshot>((ws, sa, fa) =>
                ws.ConstructSnapshotQueryAsync(TableHandle, rowRangeSet.RangeSet, columnsBitArray, sa, fa, fa));
            var snapshot = await task;
            return new TableData(_tableDefinition.Columns, snapshot);
        }

        public Task SubscribeAll()
        {
            return SubscribeAll(null);
        }

        public async Task SubscribeAll(params string[] columns)
        {
            if (!_tableDefinition.IsPreemptive)
            {
                throw new InvalidOperationException("Cannot subscribe to non-preemptive table (call Preemptive first).");
            }

            // link table update events to the source
            _context.WorkerClient.AddTableUpdateHandler(TableHandle, _tableUpdateHandler);
            _context.WorkerClient.AddTableSnapshotHandler(TableHandle, _tableSnapshotHandler);

            // start the subscription
            var bitArray = GetColumnsBitArray(columns);
            await _context.InvokeServerTask<Void?>((ws, sa, fa) =>
                ws.SubscribeAsync(TableHandle, bitArray, false, sa, fa, fa));
        }

        public async Task Subscribe(RowRangeSet rows, params string[] columns)
        {
            if (!_tableDefinition.IsPreemptive)
            {
                throw new InvalidOperationException("Cannot subscribe to non-preemptive table (call Preemptive first).");
            }

            // link table update events to the source
            _context.WorkerClient.AddTableUpdateHandler(TableHandle, _tableUpdateHandler);
            _context.WorkerClient.AddTableSnapshotHandler(TableHandle, _tableSnapshotHandler);

            // start the subscription with isViewport=true
            var bitArray = GetColumnsBitArray(columns);
            await _context.InvokeServerTask<Void?>((ws, sa, fa) =>
                ws.SubscribeAsync(TableHandle, bitArray, true, sa, fa, fa));

            // immediately specify the viewport
            await UpdateSubscription(rows, columns);
        }

        public async Task UpdateSubscription(RowRangeSet rows, string[] columns)
        {
            if (!_tableDefinition.IsPreemptive)
            {
                throw new InvalidOperationException(
                    "Cannot subscribe to non-preemptive table (call Preemptive first).");
            }

            // update the subscription
            var subscriptionRequests = new[]
            {
                new TableSubscriptionRequest
                {
                    SubscriptionId = 1,
                    Columns = GetColumnsBitArray(columns),
                    Rows = rows.RangeSet
                }
            };
            await _context.InvokeServerTask<Void?>((ws, sa, fa) =>
                ws.UpdateSubscriptionAsync(TableHandle, subscriptionRequests, sa, fa, fa));
        }

        public Task Unsubscribe()
        {
            _context.WorkerClient.RemoveTableUpdateHandler(TableHandle, _tableUpdateHandler);
            _context.WorkerClient.RemoveTableSnapshotHandler(TableHandle, _tableSnapshotHandler);
            return _context.InvokeServerTask<Void?>((ws, sa, fa) => ws.UnsubscribeAsync(TableHandle, sa, fa, fa));
        }
    }
}
