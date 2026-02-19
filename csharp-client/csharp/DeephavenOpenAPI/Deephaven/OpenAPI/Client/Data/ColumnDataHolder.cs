/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// An object used to send named column data to Deephaven.
    /// </summary>
    public class ColumnDataHolder
    {
        /// <summary>
        /// The column name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// If the table should be grouped by this column.
        /// </summary>
        public bool Grouped { get; }

        /// <summary>
        /// The column data.
        /// </summary>
        public IColumnData ColumnData { get; }

        public ColumnDataHolder(string name, IColumnData columnData)
        {
            Name = name;
            ColumnData = columnData;
        }

        public ColumnDataHolder(string name, bool grouped, IColumnData columnData)
        {
            Name = name;
            Grouped = grouped;
            ColumnData = columnData;
        }
    }
}
