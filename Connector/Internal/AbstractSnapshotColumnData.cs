/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
namespace Deephaven.Connector.Internal
{
    internal abstract class AbstractSnapshotColumnData<T> : SnapshotColumnData
    {
        protected T[] _data;

        public AbstractSnapshotColumnData(T[] data)
        {
            _data = data;
        }

        public abstract string GetDataTypeName();
        public abstract bool IsDBNull(int index);
        public abstract Type GetFieldType(int index);

        public virtual object GetValue(int index)
        {
            // the spec doesn't specify what to return for null, but the
            // SQL Server driver does this, so we imitate that.
            if(IsDBNull(index))
            {
                return DBNull.Value;
            }
            else
            {
                return _data[index];
            }
        }

        public virtual bool GetBoolean(int index)
        {
            throw new NotImplementedException();
        }

        public virtual byte GetByte(int index)
        {
            throw new NotImplementedException();
        }

        public virtual long GetBytes(int index, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public virtual char GetChar(int index)
        {
            throw new NotImplementedException();
        }

        public virtual long GetChars(int index, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }

        public virtual DateTime GetDateTime(int index)
        {
            throw new NotImplementedException();
        }

        public virtual decimal GetDecimal(int index)
        {
            throw new NotImplementedException();
        }

        public virtual double GetDouble(int index)
        {
            throw new NotImplementedException();
        }

        public virtual float GetFloat(int index)
        {
            throw new NotImplementedException();
        }

        public virtual Guid GetGuid(int index)
        {
            throw new NotImplementedException();
        }

        public virtual short GetInt16(int index)
        {
            throw new NotImplementedException();
        }

        public virtual int GetInt32(int index)
        {
            throw new NotImplementedException();
        }

        public virtual long GetInt64(int index)
        {
            throw new NotImplementedException();
        }

        public virtual string GetString(int index)
        {
            throw new NotImplementedException();
        }
    }
}
