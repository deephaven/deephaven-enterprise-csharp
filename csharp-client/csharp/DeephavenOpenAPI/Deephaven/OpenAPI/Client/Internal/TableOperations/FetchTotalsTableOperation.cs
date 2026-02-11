/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Internal.TableOperations
{
    internal static class FetchTotalsTableOperation
    {
        public enum AggType
        {
            /** Return the number of rows in each group. */
            Count,
            /** Return the minimum value of each group. */
            Min,
            /** Return the maximum value of each group. */
            Max,
            /** Return the sum of values in each group. */
            Sum,
            /** Return the sum of absolute values in each group. */
            AbsSum,
            /** Return the variance of values in each group. */
            Var,
            /** Return the average of values in each group. */
            Avg,
            /** Return the standard deviation of each group. */
            Std,
            /** Return the first value of each group. */
            First,
            /** Return the last value of each group. */
            Last,
            /** Return the values of each group as a DbArray. */
            Array,
            /** Only valid in a TotalsTableBuilder to indicate we should not perform any aggregation. */
            Skip
        };

        // TODO(kosak) - we neglected to export this functionality to the client!
        // Assuming our customers find this useful, let's wire up something soon.
        // Tracked in DH-11024.

        // public static Task<InitialTableDefinition> Invoke(ServerContext context, TableHandle parentHandle,
        //     TableHandle newTableHandle, string directive, string[] groupingColumns)
        // {
        //     return context.InvokeServerTask<InitialTableDefinition>((ws, sa, fa) =>
        //         ws.FetchTotalsTableAsync(parentHandle, newTableHandle, directive, groupingColumns, sa, fa));
        // }
        //
        // public static Task<InitialTableDefinition> Invoke(ServerContext context, TableHandle parentHandle,
        //     TableHandle newTableHandle, AggType aggType, string[] groupingColumns)
        // {
        //     // The tables have string attributes associated with them that follow the text format below.
        //     // The meaning of the three fields are "showTotalsByDefault", "showGrandTotalsByDefault", and
        //     // "defaultOperation".
        //     var directive = $"false,false,{aggType.ToString()};";
        //     return Invoke(context, parentHandle, newTableHandle, directive, groupingColumns);
        // }
    }
}
