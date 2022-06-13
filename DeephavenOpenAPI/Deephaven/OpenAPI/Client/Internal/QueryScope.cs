/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Linq;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Internal.TableOperations;
using Deephaven.OpenAPI.Shared.Data;

namespace Deephaven.OpenAPI.Client.Internal
{
    public class QueryScope : IQueryScope, IQueryScopeInternal
    {
        private static readonly DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const long NanosPerTick = 1_000_000 / TimeSpan.TicksPerMillisecond;

        private readonly ServerContext _serverContext;
        private readonly TableStateScope _tableStateScope;

        public QueryScope(ServerContext serverContext)
        {
            _serverContext = serverContext;
            _tableStateScope = new TableStateScope();
        }

        public void Dispose()
        {
            _tableStateScope.Dispose();
        }

        public IQueryScope NewScope()
        {
            return new QueryScope(_serverContext);
        }

        public IQueryTable Manage(IQueryTable queryTable)
        {
            // My scope, their state
            var tracker = TableStateTracker.Create(_tableStateScope, queryTable.TableState);
            return QueryTable.Create(this, tracker);
        }

        public IQueryTable BoundTable(string tableName)
        {
            var rb = MakeResultBuilder();
            FetchBoundTableOperation.Invoke(_serverContext, rb.ResultTsBuilder, tableName);
            return rb.ResultTable;
        }

        public IQueryTable IntradayTable(string ns, string tableName, string internalPartition = null, bool live = true)
        {
            var rb = MakeResultBuilder();
            FetchIntradayTableOperation.Invoke(_serverContext, rb.ResultTsBuilder, ns, tableName, internalPartition, live);
            return rb.ResultTable;
        }

        public IQueryTable HistoricalTable(string ns, string tableName)
        {
            var rb = MakeResultBuilder();
            FetchHistoricalTableOperation.Invoke(_serverContext, rb.ResultTsBuilder, ns, tableName);
            return rb.ResultTable;
        }

        public IQueryTable EmptyTable(long size, string[] columnNames, string[] columnTypes)
        {
            var rb = MakeResultBuilder();
            FetchEmptyTableOperation.Invoke(_serverContext, rb.ResultTsBuilder, size, columnNames, columnTypes);
            return rb.ResultTable;
        }

        public IQueryTable TempTable(ColumnDataHolder[] columnDataHolders)
        {
            var rb = MakeResultBuilder();
            var columnHolders = columnDataHolders.Select(
                cdh => new ColumnHolder
                {
                    Name = cdh.Name,
                    Type = cdh.ColumnData.Internal.GetColumnType(),
                    Grouped = cdh.Grouped,
                    ColumnData = cdh.ColumnData.Internal.GetColumnData()
                }).ToArray();
            FetchNewTableOperation.Invoke(_serverContext, rb.ResultTsBuilder, columnHolders);
            return rb.ResultTable;
        }

        public IQueryTable TimeTable(long startTimeNanos, long periodNanos)
        {
            var rb = MakeResultBuilder();
            FetchTimeTableOperation.Invoke(_serverContext, rb.ResultTsBuilder, startTimeNanos, periodNanos);
            return rb.ResultTable;
        }

        public IQueryTable TimeTable(long periodNanos)
        {
            return TimeTable(-1L, periodNanos);
        }

        public IQueryTable TimeTable(DateTime startTime, TimeSpan period)
        {
            var startTimeNanos = (startTime.Ticks - EpochTime.Ticks) * NanosPerTick;
            var periodNanos = period.Ticks * NanosPerTick;
            return TimeTable(startTimeNanos, periodNanos);
        }

        public IQueryTable TimeTable(TimeSpan period)
        {
            var periodNanos = (int)(period.Ticks * NanosPerTick);
            return TimeTable(-1L, periodNanos);
        }

        public IQueryTable CatalogTable()
        {
            var rb = MakeResultBuilder();
            CatalogTableOperation.Invoke(_serverContext, rb.ResultTsBuilder);
            return rb.ResultTable;
        }

        public async Task<DatabaseCatalog> GetDatabaseCatalogTask(bool systemNamespaces = true, bool userNamespaces = true,
            string namespaceRegex = null, string tableRegex = null)
        {
            var catTask = _serverContext.InvokeServerTask<Catalog>((ws, sa, fa) =>
                ws.GetCatalogAsync(systemNamespaces, userNamespaces, namespaceRegex, tableRegex, sa, fa, fa));
            var cat = await catTask;
            return new DatabaseCatalog(cat);
        }

        private ResultBuilder MakeResultBuilder()
        {
            var tsb = TableStateBuilder.Create(_tableStateScope, _serverContext);
            var qt = QueryTable.Create(this, tsb.TableStateTracker);
            return new ResultBuilder(tsb, qt);
        }

        IQueryScopeInternal IQueryScope.Internal => this;
        ServerContext IQueryScopeInternal.Context => _serverContext;
        TableStateScope IQueryScopeInternal.TableStateScope => _tableStateScope;
    }
}
