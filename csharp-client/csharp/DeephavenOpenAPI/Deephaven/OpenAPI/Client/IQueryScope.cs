/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Threading.Tasks;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Internal;
using Deephaven.OpenAPI.Core.API.Util;

namespace Deephaven.OpenAPI.Client
{
    public interface IQueryScope : IDisposable
    {
        IQueryScope NewScope();
        IQueryTable Manage(IQueryTable outer);

        IQueryTable BoundTable(string name);
        IQueryTable IntradayTable(string ns, string name, string internalPartition = null, bool live = true);
        IQueryTable HistoricalTable(string ns, string name);
        IQueryTable EmptyTable(long size, string[] columnNames, string[] columnTypes);
        IQueryTable TempTable(ColumnDataHolder[] columnDataHolders);
        IQueryTable TimeTable(long intervalNanos);
        IQueryTable TimeTable(long startTimeNanos, long intervalMillis);
        IQueryTable TimeTable(DateTime startTime, TimeSpan periodInterval);
        IQueryTable TimeTable(TimeSpan periodInterval);
        IQueryTable CatalogTable();

        Task<DatabaseCatalog> GetDatabaseCatalogTask(
            bool systemNamespaces = true, bool userNamespaces = true,
            string namespaceRegex = null, string tableRegex = null);

        /// <summary>
        /// Deephaven internal operations. Clients should not use.
        /// </summary>
        IQueryScopeInternal Internal { get; }
    }

    public static class QueryScope_Extensions
    {
        public static DatabaseCatalog GetDatabaseCatalog(this IQueryScope self,
            bool systemNamespaces = true, bool userNamespaces = true,
            string namespaceRegex = null, string tableRegex = null)
        {
            return ExceptionUtil.ResultOrUnwrappedException(self.GetDatabaseCatalogTask(systemNamespaces,
                userNamespaces, namespaceRegex, tableRegex));
        }
    }
}
