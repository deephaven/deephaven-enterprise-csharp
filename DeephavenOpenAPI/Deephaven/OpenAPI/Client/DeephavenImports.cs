/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;

namespace Deephaven.OpenAPI.Client
{
    public static class DeephavenImports
    {
        public static AggregateCombo AggCombo(params Aggregate[] aggregates) => AggregateCombo.Create(aggregates);

        public static Aggregate AggFirst(params string[] columns) => Aggregate.First(columns);
        public static Aggregate AggLast(params string[] columns) => Aggregate.Last(columns);
        public static Aggregate AggSum(params string[] columns) => Aggregate.Sum(columns);
        public static Aggregate AggAvg(params string[] columns) => Aggregate.Avg(columns);
        public static Aggregate AggStd(params string[] columns) => Aggregate.Std(columns);
        public static Aggregate AggVar(params string[] columns) => Aggregate.Var(columns);
        public static Aggregate AggMed(params string[] columns) => Aggregate.Med(columns);
        public static Aggregate AggMin(params string[] columns) => Aggregate.Min(columns);
        public static Aggregate AggMax(params string[] columns) => Aggregate.Max(columns);
        public static Aggregate AggCount(string resultColumn) => Aggregate.Count(resultColumn);
        public static Aggregate AggArray(params string[] columns) => Aggregate.Array(columns);
        public static Aggregate AggPct(double percentile, params string[] columns) =>
            Aggregate.Pct(percentile, columns);
        public static Aggregate AggWAvg(string resultColumn, params string[] columns) =>
            Aggregate.WAvg(resultColumn, columns);
        public static Aggregate AggAbsSum(params string[] columns) => Aggregate.AbsSum(columns);


        public static Aggregate AggFirst(params ISelectColumn[] columns) => Aggregate.First(columns);
        public static Aggregate AggLast(params ISelectColumn[] columns) => Aggregate.Last(columns);
        public static Aggregate AggSum(params ISelectColumn[] columns) => Aggregate.Sum(columns);
        public static Aggregate AggAvg(params ISelectColumn[] columns) => Aggregate.Avg(columns);
        public static Aggregate AggStd(params ISelectColumn[] columns) => Aggregate.Std(columns);
        public static Aggregate AggVar(params ISelectColumn[] columns) => Aggregate.Var(columns);
        public static Aggregate AggMed(params ISelectColumn[] columns) => Aggregate.Med(columns);
        public static Aggregate AggMin(params ISelectColumn[] columns) => Aggregate.Min(columns);
        public static Aggregate AggMax(params ISelectColumn[] columns) => Aggregate.Max(columns);
        public static Aggregate AggArray(params ISelectColumn[] columns) => Aggregate.Array(columns);
        public static Aggregate AggPct(double percentile, params ISelectColumn[] columns) =>
            Aggregate.Pct(percentile, columns);
        public static Aggregate AggWAvg(ISelectColumn resultColumn, params ISelectColumn[] columns) =>
            Aggregate.WAvg(resultColumn, columns);
        public static Aggregate AggAbsSum(params ISelectColumn[] columns) =>
            Aggregate.AbsSum(columns);

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggFirst() => Aggregate.First();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggLast() => Aggregate.Last();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggSum() => Aggregate.Sum();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggAvg() => Aggregate.Avg();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggStd() => Aggregate.Std();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggVar() => Aggregate.Var();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggMed() => Aggregate.Med();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggMin() => Aggregate.Min();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggMax() => Aggregate.Max();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggArray() => Aggregate.Array();

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggPct(double percentile) => Aggregate.Pct(percentile);

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggWAvg(string resultColumn) => Aggregate.WAvg(resultColumn);

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between the string[] and
        /// ISelectColumn[] versions of the method, if the caller were ever to invoke it with no arguments.
        /// TODO(kosak): decide what you would like to do if the no-arg invocation is nonsensical.
        /// </summary>
        public static Aggregate AggAbsSum() => Aggregate.AbsSum();
    }
}
