/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System.Numerics;
using Deephaven.OpenAPI.Core.RPC.Serialization.Java.Math;

namespace Deephaven.OpenAPI.Shared.Data.Columns
{
    public interface IColumnDataVisitor<in T, out R>
    {
        R Visit(BigDecimalArrayColumnData self, T arg);
        R Visit(BigIntegerArrayColumnData self, T arg);
        R Visit(ByteArrayArrayColumnData self, T arg);
        R Visit(ByteArrayColumnData self, T arg);
        R Visit(CharArrayArrayColumnData self, T arg);
        R Visit(CharArrayColumnData self, T arg);
        R Visit(DoubleArrayArrayColumnData self, T arg);
        R Visit(DoubleArrayColumnData self, T arg);
        R Visit(FloatArrayArrayColumnData self, T arg);
        R Visit(FloatArrayColumnData self, T arg);
        R Visit(IntArrayArrayColumnData self, T arg);
        R Visit(IntArrayColumnData self, T arg);
        R Visit(LocalDateArrayColumnData self, T arg);
        R Visit(LocalTimeArrayColumnData self, T arg);
        R Visit(LongArrayArrayColumnData self, T arg);
        R Visit(LongArrayColumnData self, T arg);
        R Visit(ShortArrayArrayColumnData self, T arg);
        R Visit(ShortArrayColumnData self, T arg);
        R Visit(StringArrayArrayColumnData self, T arg);
        R Visit(StringArrayColumnData self, T arg);
    }
    /// <summary>
    /// Base class for ColumnData DTO types. The only reason we don't generate this code is for the GetData()
    /// method, making it easier to treat these objects in a generic way and still get at the data.
    /// </summary>
    public abstract class ColumnData
    {
        public abstract object GetData();
        public abstract R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg);
    }

    public abstract class ColumnData<T> : ColumnData
    {
        public T[] Data { get; set; }

        public sealed override object GetData()
        {
            return Data;
        }
    }

    public class BigDecimalArrayColumnData : ColumnData<BigDecimal?>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class BigIntegerArrayColumnData : ColumnData<BigInteger?>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class ByteArrayArrayColumnData : ColumnData<sbyte[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class ByteArrayColumnData : ColumnData<sbyte>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class CharArrayArrayColumnData : ColumnData<char[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class CharArrayColumnData : ColumnData<char>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class DoubleArrayArrayColumnData : ColumnData<double[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class DoubleArrayColumnData : ColumnData<double>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class FloatArrayArrayColumnData : ColumnData<float[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class FloatArrayColumnData : ColumnData<float>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class IntArrayArrayColumnData : ColumnData<int[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class IntArrayColumnData : ColumnData<int>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class LocalDateArrayColumnData : ColumnData<LocalDate>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class LocalTimeArrayColumnData : ColumnData<LocalTime>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class LongArrayArrayColumnData : ColumnData<long[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class LongArrayColumnData : ColumnData<long>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class ShortArrayArrayColumnData : ColumnData<short[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class ShortArrayColumnData : ColumnData<short>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class StringArrayArrayColumnData : ColumnData<string[]>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
    public class StringArrayColumnData : ColumnData<string>
    {
        public sealed override R AcceptVisitor<T, R>(IColumnDataVisitor<T, R> visitor, T arg)
        {
            return visitor.Visit(this, arg);
        }
    }
}
