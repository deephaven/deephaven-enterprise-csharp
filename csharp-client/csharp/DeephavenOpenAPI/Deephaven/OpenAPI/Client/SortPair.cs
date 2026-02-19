/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Client.Fluent;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// An object representing a sort request on a column. Columns can be
    /// sorted in ascending or descending order, and by absolute value if
    /// requested.
    /// </summary>
    public class SortPair
    {
        /// <summary>
        /// Column name
        /// </summary>
        public string Column { get; }
        /// <summary>
        /// Sort direction
        /// </summary>
        public SortDirection Direction { get; }
        /// <summary>
        /// Whether the sort should be done by absolute value.
        /// </summary>
        public bool Abs { get; }

        /// <summary>
        /// Creates a SortPair bound to "column" with Ascending direction and optional absolute value handling.
        /// </summary>
        /// <param name="column">The name of the column to be sorted</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns>The SortPair</returns>
        public static SortPair Ascending(string column, bool abs = false)
        {
            return new SortPair(column, SortDirection.Ascending, abs);
        }
        /// <summary>
        /// Creates a SortPair bound to "column" with Ascending direction and optional absolute value handling.
        /// </summary>
        /// <param name="column">The IColumn to be sorted</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns>The SortPair</returns>
        public static SortPair Ascending(IColumn column, bool abs = false)
        {
            return Ascending(column.ToIrisRepresentation(), abs);
        }

        /// <summary>
        /// Creates a SortPair bound to "column" with Descending direction and optional absolute value handling.
        /// </summary>
        /// <param name="column">The name of the column to be sorted</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns>The SortPair</returns>
        public static SortPair Descending(string column, bool abs = false)
        {
            return new SortPair(column, SortDirection.Descending, abs);
        }
        /// <summary>
        /// Creates a SortPair bound to "column" with Descending direction and optional absolute value handling.
        /// </summary>
        /// <param name="column">The IColumn to be sorted</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns>The SortPair</returns>
        public static SortPair Descending(IColumn column, bool abs = false)
        {
            return Descending(column.ToIrisRepresentation(), abs);
        }

        /// <summary>
        /// Creates a SortPair bound to "column" with specified direction and absolute value handling.
        /// </summary>
        /// <param name="column">The name of the column to be sorted</param>
        /// <param name="sortDirection">The sort direction</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns>The SortPair</returns>
        public SortPair(string column, SortDirection sortDirection, bool abs) =>
            (Column, Direction, Abs) = (column, sortDirection, abs);

        /// <summary>
        /// Creates a SortPair bound to "column" with specified direction and absolute value handling.
        /// </summary>
        /// <param name="column">The IColumn to be sorted</param>
        /// <param name="sortDirection">The sort direction</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns>The SortPair</returns>
        public SortPair(IColumn column, SortDirection sortDirection, bool abs) :
            this(column.ToIrisRepresentation(), sortDirection, abs)
        {
        }
    }

    public static class SortPair_Extensions
    {
        /// <summary>
        /// Creates a SortPair bound to "this" IColumn with Ascending direction and optional absolute value handling.
        /// </summary>
        /// <param name="self">The IColumn to be sorted</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns></returns>
        public static SortPair Ascending(this IColumn self, bool abs = false)
        {
            return SortPair.Ascending(self, abs);
        }

        /// <summary>
        /// Creates a SortPair bound to "this" IColumn with Descending direction and optional absolute alue handling.
        /// </summary>
        /// <param name="self">The IColumn to be sorted</param>
        /// <param name="abs">Whether item should be sorted by absolute value</param>
        /// <returns></returns>
        public static SortPair Descending(this IColumn self, bool abs = false)
        {
            return SortPair.Descending(self, abs);
        }
    }
}
