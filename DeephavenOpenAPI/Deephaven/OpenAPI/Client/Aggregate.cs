/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Linq;
using Deephaven.OpenAPI.Client.Data;
using Deephaven.OpenAPI.Client.Fluent;
using Deephaven.OpenAPI.Client.Internal;
using Deephaven.OpenAPI.Shared.Batch.Aggregates;

namespace Deephaven.OpenAPI.Client
{
    /// <summary>
    /// Use an Aggregate object to specify each aggregate that comprises an
    /// <see cref="AggregateCombo"/> object. There are a variety of factory
    /// methods here; however, it is typically easier to use the convenience
    /// methods in <see cref="DeephavenImports"/>, like so:
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
    public class Aggregate
    {
        /// <summary>
        /// Gets the internal Deephaven data structure describing this object.
        /// </summary>
        internal AggregateDescriptor Descriptor { get; }

        private Aggregate(AggregateDescriptor aggregateDescriptor) => Descriptor = aggregateDescriptor;

        /// <summary>
        /// Calculates the sum of the absolute value of the values in each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate AbsSum(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.AbsSum,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Calculates the sum of the absolute value of the values in each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate AbsSum(params ISelectColumn[] columns)
        {
            return AbsSum(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="AbsSum(string[])"/> and <see cref="AbsSum(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate AbsSum()
        {
            return AbsSum(new string[0]);
        }

        /// <summary>
        /// Combines the values in each specified column into an array.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Array(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Array,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Combines the values in each specified column into an array.
        /// </summary>
        /// <param name="columns">Column names or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Array(params ISelectColumn[] columns)
        {
            return Array(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Array(string[])"/> and <see cref="Array(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Array()
        {
            return Array(new string[0]);
        }

        /// <summary>
        /// Calculates the average of each specified column into an array.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Avg(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Avg,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Calculates the average of each specified column into an array.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Avg(params ISelectColumn[] columns)
        {
            return Avg(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Avg(string[])"/> and <see cref="Avg(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Avg()
        {
            return Avg(new string[0]);
        }

        /// <summary>
        /// Calculates the number of rows in the specified column.
        /// </summary>
        /// <param name="resultColumn">Column name or rename expression like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Count(string resultColumn)
        {
            return new Aggregate(new AggregateDescriptor
            {
                ColumnName = resultColumn,
                AggregateType = AggType.Count
            });
        }

        /// <summary>
        /// Calculates the average of each specified column into an array.
        /// </summary>
        /// <param name="resultColumn">Columns or column rename expression like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Count(ISelectColumn resultColumn)
        {
            return Avg(resultColumn.ToIrisRepresentation());
        }

        /// <summary>
        /// Gets the first row of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate First(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.First,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Gets the first row of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate First(params ISelectColumn[] columns)
        {
            return First(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="First(string[])"/> and <see cref="First(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate First()
        {
            return First(new string[0]);
        }

        /// <summary>
        /// Gets the last row of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Last(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Last,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Gets the last row of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Last(params ISelectColumn[] columns)
        {
            return Last(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Last(string[])"/> and <see cref="Last(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Last()
        {
            return Last(new string[0]);
        }

        /// <summary>
        /// Gets the maximum value of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Max(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Max,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Gets the maximum value of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Max(params ISelectColumn[] columns)
        {
            return Max(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Max(string[])"/> and <see cref="Max(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Max()
        {
            return Max(new string[0]);
        }

        /// <summary>
        /// Gets the median value of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Med(params string[] columns)
        {
            return Pct(0.5d, true, columns);
        }

        /// <summary>
        /// Gets the median value of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Med(params ISelectColumn[] columns)
        {
            return Med(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Med(string[])"/> and <see cref="Med(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Med()
        {
            return Med(new string[0]);
        }

        /// <summary>
        /// Gets the minimum value of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Min(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Min,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Gets the mininum value of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Min(params ISelectColumn[] columns)
        {
            return Min(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Min(string[])"/> and <see cref="Min(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Min()
        {
            return Min(new string[0]);
        }

        /// <summary>
        /// Gets the value at the given percentile for each specified column.
        /// </summary>
        /// <param name="percentile">The specified percentile</param>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Pct(double percentile, params string[] columns)
        {
            return Pct(percentile, true, columns);
        }

        /// <summary>
        /// Gets the value at the given percentile for each specified column.
        /// </summary>
        /// <param name="percentile">The specified percentile</param>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Pct(double percentile, params ISelectColumn[] columns)
        {
            return Pct(percentile, columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Pct(double,string[])"/> and <see cref="Pct(double,ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Pct(double percentile)
        {
            return Pct(percentile, new string[0]);
        }

        /// <summary>
        /// Gets the value at the given percentile for each specified column.
        /// </summary>
        /// <param name="percentile">The specified percentile</param>
        /// <param name="avgMedian">TODO (kosak)</param>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Pct(double percentile, bool avgMedian, params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Percentile,
                Percentile = new PercentileDescriptor
                {
                    Percentile = percentile,
                    AvgMedian = avgMedian
                },
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Gets the value at the given percentile for each specified column.
        /// </summary>
        /// <param name="percentile">The specified percentile</param>
        /// <param name="avgMedian">TODO (kosak)</param>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Pct(double percentile, bool avgMedian, params ISelectColumn[] columns)
        {
            return Pct(percentile, avgMedian, columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Pct(double,bool,string[])"/> and <see cref="Pct(double,bool,ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Pct(double percentile, bool avgMedian)
        {
            return Pct(percentile, avgMedian, new string[0]);
        }

        /// <summary>
        /// Calculates the standard deviation for each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Std(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Std,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Calculates the standard deviation for each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Std(params ISelectColumn[] columns)
        {
            return Std(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Std(string[])"/> and <see cref="Std(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Std()
        {
            return Std(new string[0]);
        }

        /// <summary>
        /// Calculates the sum of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Sum(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Sum,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Calculates the sum of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Sum(params ISelectColumn[] columns)
        {
            return Sum(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Sum(string[])"/> and <see cref="Sum(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Sum()
        {
            return Sum(new string[0]);
        }

        /// <summary>
        /// Calculates the variance of each specified column.
        /// </summary>
        /// <param name="columns">Column names or rename expressions like "Foo=Bar"</param>
        /// <returns></returns>
        public static Aggregate Var(params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.Var,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Calculates the variance of each specified column.
        /// </summary>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate Var(params ISelectColumn[] columns)
        {
            return Var(columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="Var(string[])"/> and <see cref="Var(ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate Var()
        {
            return Var(new string[0]);
        }

        /// <summary>
        /// Calculates the weighted average of the specified columns.
        /// </summary>
        /// <param name="weightColumn">The weight column. This is mulitplied by the values in each other column."</param>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate WAvg(string weightColumn, params string[] columns)
        {
            return new Aggregate(new AggregateDescriptor
            {
                AggregateType = AggType.WeightedAvg,
                ColumnName = weightColumn,
                MatchPairs = columns
            });
        }

        /// <summary>
        /// Calculates the weighted average of the specified columns.
        /// </summary>
        /// <param name="weightColumn">The weight column. This is mulitplied by the values in each other column."</param>
        /// <param name="columns">Columns or column rename expressions like foo.As("Bar")</param>
        /// <returns></returns>
        public static Aggregate WAvg(ISelectColumn weightColumn, params ISelectColumn[] columns)
        {
            return WAvg(weightColumn.ToIrisRepresentation(), columns.ToIrisRepArray());
        }

        /// <summary>
        /// This method exists to resolve the ambiguity that would otherwise exist between
        /// <see cref="WAvg(string,string[])"/> and <see cref="WAvg(ISelectColumn,ISelectColumn[])"/>
        /// if the caller were ever to invoke it with no arguments.
        /// </summary>
        public static Aggregate WAvg(string weightColumn)
        {
            return WAvg(weightColumn, new string[0]);
        }
    }
}
