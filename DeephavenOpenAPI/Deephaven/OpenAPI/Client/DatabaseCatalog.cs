/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System.Linq;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// A catalog of available Deephaven database tables.
    /// </summary>
    public class DatabaseCatalog
    {
        /// <summary>
        /// The individual tables
        /// </summary>
        public DatabaseCatalogTable[] Tables { get; }

        internal DatabaseCatalog(Shared.Data.Catalog catalog)
        {
            Tables = catalog.Tables.Select(t => new DatabaseCatalogTable(t)).ToArray();
        }
    }

    public class DatabaseCatalogTable
    {
        private readonly Shared.Data.Catalog.CatalogTable _catalogTable;

        internal DatabaseCatalogTable(Shared.Data.Catalog.CatalogTable catalogTable) =>
            this._catalogTable = catalogTable;

        public string NamespaceSet => _catalogTable.NamespaceSet;
        public string Namespace => _catalogTable.Namespace;
        public string TableName => _catalogTable.TableName;
    }
}
