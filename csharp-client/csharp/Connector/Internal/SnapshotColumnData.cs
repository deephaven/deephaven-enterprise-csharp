/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal interface SnapshotColumnData
    {
        string GetDataTypeName();
        bool IsDBNull(int index);
        Type GetFieldType(int index);
        bool GetBoolean(int index);
        byte GetByte(int index);
        long GetBytes(int index, long dataOffset, byte[] buffer, int bufferOffset, int length);
        char GetChar(int index);
        long GetChars(int index, long dataOffset, char[] buffer, int bufferOffset, int length);
        DateTime GetDateTime(int index);
        decimal GetDecimal(int index);
        double GetDouble(int index);
        float GetFloat(int index);
        Guid GetGuid(int index);
        short GetInt16(int index);
        int GetInt32(int index);
        long GetInt64(int index);
        string GetString(int index);
        object GetValue(int index);
    }
}
