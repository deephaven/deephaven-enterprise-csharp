/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

using System.Numerics;

namespace Deephaven.OpenAPI.Client.Data
{
    public interface IColumnData
    {
        int Length { get; }
        bool IsNull(int row);

        bool GetBoolean(int row);
        bool? GetNullableBoolean(int row);
        int GetInt32(int row);
        int? GetNullableInt32(int row);
        long GetInt64(int row);
        long? GetNullableInt64(int row);
        sbyte GetByte(int row);
        sbyte? GetNullableByte(int row);
        short GetInt16(int row);
        short? GetNullableInt16(int row);
        double GetDouble(int row);
        double? GetNullableDouble(int row);
        float GetFloat(int row);
        float? GetNullableFloat(int row);
        decimal? GetDecimal(int row);
        DHDecimal? GetDHDecimal(int row);
        char GetChar(int row);
        char? GetNullableChar(int row);
        DHDate GetDHDate(int row);
        DHTime GetDHTime(int row);
        DBDateTime GetDBDateTime(int row);
        BigInteger? GetBigInteger(int row);

        string GetString(int row);
        object GetObject(int row);

        IColumnDataInternal Internal { get; }
    }

    public interface IColumnDataInternal
    {
        Deephaven.OpenAPI.Shared.Data.Columns.ColumnData GetColumnData();
        string GetColumnType();
    }
}
