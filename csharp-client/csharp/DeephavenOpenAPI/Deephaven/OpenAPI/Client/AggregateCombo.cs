/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// This class specifies a combined aggregate operation, which allows the caller to specify
    /// multiple aggregate operations on a source table. For convenience, the caller may
    /// the <see cref="DeephavenImports.AggCombo" /> method, with a static import.
    /// Example:
    /// <code>
    /// using static Deephaven.OpenAPI.Client.DeephavenImports;
    /// ...
    /// var t2 = t.View(close)
    ///   .By(AggCombo(
    ///     AggAvg(close.As("AvgClose")),
    ///     AggSum(close.As("SumClose")),
    ///     AggMin(close.As("MinClose")),
    ///     AggMax(close.As("MaxClose")),
    ///     AggCount("Count")));
    /// </code>
    /// </summary>
    public class AggregateCombo
    {
        public Aggregate[] Aggregates { get; }

        private AggregateCombo(params Aggregate[] aggregates) => Aggregates = aggregates;

        public static AggregateCombo Create(params Aggregate[] aggregates) => new AggregateCombo(aggregates);
    }
}
