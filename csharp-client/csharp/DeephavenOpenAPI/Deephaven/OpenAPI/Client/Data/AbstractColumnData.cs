/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System;
using System.Numerics;
using Deephaven.OpenAPI.Client.Internal;

namespace Deephaven.OpenAPI.Client.Data
{
    public abstract class AbstractColumnData : IColumnData, IColumnDataInternal
    {
        public abstract int Length { get; }
        public abstract bool IsNull(int row);

        public virtual bool GetBoolean(int row)
        {
            throw new NotSupportedException();
        }

        public virtual bool? GetNullableBoolean(int row)
        {
            throw new NotSupportedException();
        }

        public virtual int GetInt32(int row)
        {
            throw new NotSupportedException();
        }

        public virtual int? GetNullableInt32(int row)
        {
            throw new NotSupportedException();
        }

        public virtual long GetInt64(int row)
        {
            throw new NotSupportedException();
        }

        public virtual long? GetNullableInt64(int row)
        {
            throw new NotSupportedException();
        }

        public virtual sbyte GetByte(int row)
        {
            throw new NotSupportedException();
        }

        public virtual sbyte? GetNullableByte(int row)
        {
            throw new NotSupportedException();
        }

        public virtual short GetInt16(int row)
        {
            throw new NotSupportedException();
        }

        public virtual short? GetNullableInt16(int row)
        {
            throw new NotSupportedException();
        }

        public virtual double GetDouble(int row)
        {
            throw new NotSupportedException();
        }

        public virtual double? GetNullableDouble(int row)
        {
            throw new NotSupportedException();
        }

        public virtual float GetFloat(int row)
        {
            throw new NotSupportedException();
        }

        public virtual float? GetNullableFloat(int row)
        {
            throw new NotSupportedException();
        }

        public virtual decimal? GetDecimal(int row)
        {
            throw new NotSupportedException();
        }

        public virtual DHDecimal? GetDHDecimal(int row)
        {
            throw new NotSupportedException();
        }

        public virtual char GetChar(int row)
        {
            throw new NotSupportedException();
        }

        public virtual char? GetNullableChar(int row)
        {
            throw new NotSupportedException();
        }

        public virtual DHDate GetDHDate(int row)
        {
            throw new NotSupportedException();
        }

        public virtual DHTime GetDHTime(int row)
        {
            throw new NotSupportedException();
        }

        public virtual DBDateTime GetDBDateTime(int row)
        {
            throw new NotSupportedException();
        }

        public virtual BigInteger? GetBigInteger(int row)
        {
            throw new NotSupportedException();
        }

        public abstract string GetString(int row);
        public abstract Object GetObject(int row);

        public IColumnDataInternal Internal => this;

        Deephaven.OpenAPI.Shared.Data.Columns.ColumnData IColumnDataInternal.GetColumnData()
        {
            return InternalGetColumnData();
        }

        string IColumnDataInternal.GetColumnType()
        {
            return InternalGetColumnType();
        }

        protected abstract Deephaven.OpenAPI.Shared.Data.Columns.ColumnData InternalGetColumnData();
        protected abstract string InternalGetColumnType();
    }

    /// <summary>
    /// <para>The <see cref="AbstractColumnData{DT,CDT}"/> object provides access to data associated
    /// with a particular Deephaven table column. Both <see cref="TableData"/>
    /// and <see cref="ITableUpdate"/> objects use this abstraction to provide
    /// access to table data resulting from one-time requests or subscription
    /// updates, respectively.</para>
    /// <para>Each concrete column type is represented by a subclass of this
    /// type. Depending on the column type, a different subset of "getter"
    /// methods is implemented.</para>
    /// </summary>
    /// <typeparam name="DT">The type representing column values for this column data type</typeparam>
    /// <typeparam name="CDT">The internal Open API column data type</typeparam>
    public abstract class AbstractColumnData<EffectiveType, UnderlyingType, ColumnDataType>
        : AbstractColumnData where ColumnDataType : Deephaven.OpenAPI.Shared.Data.Columns.ColumnData<UnderlyingType>, new()
    {
        protected ColumnDataType ColumnData;

        protected AbstractColumnData()
        {
        }

        protected AbstractColumnData(ColumnDataType columnData)
        {
            ColumnData = columnData;
        }

        protected AbstractColumnData(int size)
        {
            ColumnData = new ColumnDataType {Data = new UnderlyingType[size]};
        }

        protected AbstractColumnData(UnderlyingType[] data)
        {
            ColumnData = new ColumnDataType {Data = data};
        }

        protected sealed override Deephaven.OpenAPI.Shared.Data.Columns.ColumnData InternalGetColumnData()
        {
            return ColumnData;
        }

        public sealed override int Length => ColumnData.Data.Length;

        /// <summary>
        /// Default implementation, if DT differs from the wrapped type, must be overridden.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public abstract EffectiveType GetValue(int row);

        public abstract void SetValue(int row, EffectiveType value);

        public EffectiveType this[int i]
        {
            get => GetValue(i);
            set => SetValue(i, value);
        }

        public sealed override string GetString(int row)
        {
            return IsNull(row) ? null : GetValue(row).ToString();
        }

        public sealed override object GetObject(int row)
        {
            if (IsNull(row))
            {
                return null;
            }
            return GetValue(row);
        }

    }

    /// <summary>
    /// <para>The <see cref="AbstractColumnData{DT,CDT}"/> object provides access to data associated
    /// with a particular Deephaven table column. Both <see cref="TableData"/>
    /// and <see cref="ITableUpdate"/> objects use this abstraction to provide
    /// access to table data resulting from one-time requests or subscription
    /// updates, respectively.</para>
    /// <para>Each concrete column type is represented by a subclass of this
    /// type. Depending on the column type, a different subset of "getter"
    /// methods is implemented.</para>
    /// </summary>
    /// <typeparam name="DataType">The type representing column values for this column data type</typeparam>
    /// <typeparam name="ColumnDataType">The internal Open API column data type</typeparam>
    public abstract class
        AbstractColumnData<DataType, ColumnDataType> : AbstractColumnData<DataType, DataType, ColumnDataType>
        where ColumnDataType : Deephaven.OpenAPI.Shared.Data.Columns.ColumnData<DataType>, new()
    {
        internal AbstractColumnData()
        {
        }

        internal AbstractColumnData(ColumnDataType columnData) : base(columnData)
        {
        }

        protected AbstractColumnData(int size) : base(size)
        {
        }

        protected AbstractColumnData(DataType[] data) : base(data)
        {
        }

        /// <summary>
        /// Default implementation, if DT differs from the wrapped type, must be overridden.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public sealed override DataType GetValue(int row)
        {
            return ColumnData.Data[row];
        }

        public sealed override void SetValue(int row, DataType value)
        {
            ColumnData.Data[row] = value;
        }
    }
}
