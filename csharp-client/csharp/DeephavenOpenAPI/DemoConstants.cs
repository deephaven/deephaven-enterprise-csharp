/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
namespace Deephaven
{
    /// <summary>
    /// This class contains certain constants (e.g. table namespaces, table names) used by Deephaven demo code.
    /// </summary>
    public static class DemoConstants
    {
        /// <summary>
        /// This is the namespace for our example historical table.
        /// </summary>
        public const string HistoricalNamespace = "LearnDeephaven";
        /// <summary>
        /// This is an example historical (aka non-ticking or static) table.
        /// </summary>
        public const string HistoricalTable = "EODTrades";

        /// <summary>
        /// This is the namespace for our example intraday table.
        /// </summary>
        public const string IntradayNamespace = "DbInternal";
        /// <summary>
        /// This is an example intraday (aka ticking or dynamic) table.
        /// </summary>
        public const string IntradayTable = "ProcessEventLog";
    }
}
