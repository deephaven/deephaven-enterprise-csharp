/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections;
using System.Data.Common;
using Deephaven.Connector.Internal;

namespace Deephaven.Connector
{
    /// <summary>
    /// Reads a forward-only stream of rows from a Deephaven data source.
    /// </summary>
    /// <remarks>
    /// The class incrementally reads a consistent snapshot of a Deephaven
    /// table (in chunks consistent with the specified <see cref="FetchSize">FetchSize</see>).
    /// Ticking tables can be queried, but it does not provide streaming data.
    /// </remarks>
    public class DeephavenDataReader : DbDataReader
    {
        private ConsoleConnection.SnapshotQueryResult[] _snapshotQueryResults;

        private int _currentResultIdx;
        private long _currentOffset; // offset of the current snapshot into the current query result table
        private long _currentPos = -1;

        private SnapshotColumnData[] _currentColumnData;
        private long _currentColumnDataSize;

        private ConsoleConnection.SnapshotQueryResult CurrentQueryResult => _snapshotQueryResults[_currentResultIdx];
        private long CurrentTableSize => _snapshotQueryResults[_currentResultIdx].InitialTableDefinition.Size;
        private int CurrentRowIndex => (int)(_currentPos - _currentOffset);

        /// <summary>
        /// Constructs a new instance of <see cref="DeephavenDataReader"/>.
        /// Internal only, called by the command object.
        /// </summary>
        /// <param name="snapshotQueryResults"></param>
        internal DeephavenDataReader(ConsoleConnection.SnapshotQueryResult[] snapshotQueryResults)
        {
            _snapshotQueryResults = snapshotQueryResults;
            _currentResultIdx = 0;
        }

        /// <summary>
        /// Gets or sets the fetch size. Rows will be copied from the server
        /// in chunks according to this value. Default value is 10,000.
        /// Larger or smaller values may be optimal depending on memory, network
        /// and table properties (encoded size of data).
        /// </summary>
        public int FetchSize { get; set; } = 10_000;

        /// <summary>
        /// Closes the <see cref="DeephavenDataReader"/> object.
        /// </summary>
        public override void Close()
        {
            base.Close();
            _snapshotQueryResults = null;
            _currentColumnData = null;
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of Object.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object this[int ordinal] => GetValue(ordinal);

        /// <summary>
        /// Gets the value of the specified column as an instance of Object.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The value of the specified column.</returns>
        public override object this[string name] => GetValue(GetOrdinal(name));

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// Depth greater than zero not presently supported, will always return zero.
        /// </summary>
        public override int Depth => 0;

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public override int FieldCount => CurrentQueryResult.ColumnCount;

        /// <summary>
        /// Gets a value that indicates whether this <see cref="DbDataReader"/> contains one or more rows.
        /// </summary>
        public override bool HasRows => CurrentQueryResult.InitialTableDefinition.Size > 0;

        /// <summary>
        /// Gets a value indicating whether the <see cref="DbDataReader"/> is closed.
        /// </summary>
        public override bool IsClosed => _snapshotQueryResults == null;

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the statement.
        /// Since updates are not presently supported, always returns -1.
        /// </summary>
        public override int RecordsAffected => -1;

        /// <summary>
        /// Advances the reader to the next result when reading the results of a batch of statements.
        /// </summary>
        /// <returns><code>true</code> if there are more result sets; otherwise, <code>false</code>.</returns>
        public override bool NextResult()
        {

            if (_currentResultIdx >= _snapshotQueryResults.Length-1)
            {
                return false;
            }
            else
            {
                _currentResultIdx++;
                _currentColumnData = null;
                _currentOffset = 0;
                _currentPos = -1;
                _currentColumnDataSize = 0;
                return true;
            }
        }

        /// <summary>
        /// Advances the reader to the next record in a result set.
        /// </summary>
        /// <returns><code>true</code> if there are more rows; otherwise, <code>false</code>.</returns>
        public override bool Read()
        {
            try
            {
                if (_currentPos < CurrentTableSize)
                {
                    _currentPos++;
                    if (_currentPos < CurrentTableSize)
                    {
                        if (_currentColumnData == null || _currentPos >= _currentOffset + _currentColumnDataSize)
                        {
                            // time to pull in new snapshot
                            _currentColumnData = CurrentQueryResult.GetTableData(_currentPos, _currentPos + FetchSize, out _currentColumnDataSize);
                            _currentOffset = _currentPos;
                        }
                    }
                    return _currentPos < CurrentTableSize;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error updating DataReader: " + ex, ex);
            }
            return false;
        }

        private void CheckColumnIndex(int ordinal)
        {
            if(ordinal < 0 || ordinal >= CurrentQueryResult.ColumnCount)
            {
                throw new ArgumentException("Bad column ordinal: " + ordinal);
            }
        }

        /// <summary>
        /// Returns an enumerator that can be used to iterate through the rows in the data reader.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the rows in the data reader.</returns>
        public override IEnumerator GetEnumerator()
        {
            return new DbEnumerator(this);
        }

        /// <summary>
        /// Gets a value that indicates whether the column contains nonexistent or missing values.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns><code>true</code> if the specified column is equivalent to <see cref="DBNull"/>; otherwise, <code>false</code>.</returns>
        public override bool IsDBNull(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].IsDBNull(CurrentRowIndex);
        }

        /// <summary>
        /// Gets name of the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the data type.</returns>
        public override string GetDataTypeName(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetDataTypeName();
        }

        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type of the specified column.</returns>
        public override Type GetFieldType(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetFieldType(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the name of the column, given the zero-based column ordinal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the specified column.</returns>
        public override string GetName(int ordinal)
        {
            // the method validate the index
            return CurrentQueryResult.GetColumnName(ordinal);
        }

        /// <summary>
        /// Gets the column ordinal given the name of the column.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based column ordinal.</returns>
        public override int GetOrdinal(string name)
        {
            // the method validate the index
            return CurrentQueryResult.GetColumnIndex(name);
        }

        /// <summary>
        /// Populates an array of objects with the column values of the current row.
        /// </summary>
        /// <param name="values">An array of <see cref="Object"/> into which to copy the attribute columns.</param>
        /// <returns>The number of instances of <see cref="Object"/> in the array.</returns>
        public override int GetValues(object[] values)
        {
            int nValues = Math.Min(values.Length, _currentColumnData.Length);
            for (var i = 0; i < nValues; i++)
            {
                values[i] = _currentColumnData[i].GetValue(CurrentRowIndex);
            }
            return nValues;
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override bool GetBoolean(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetBoolean(CurrentRowIndex);
        }
        /// <summary>
        /// Gets the value of the specified column as a byte.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override byte GetByte(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetByte(CurrentRowIndex);
        }

        /// <summary>
        /// Reads a specified number of bytes from the specified column starting at a specified index and writes them to a buffer starting at a specified position in the buffer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetBytes(CurrentRowIndex, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets the value of the specified column as a char.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override char GetChar(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetChar(CurrentRowIndex);
        }

        /// <summary>
        /// Reads a specified number of characters from a specified column starting at a specified index, and writes them to a buffer starting at a specified position.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index within the row from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which to copy the data.</param>
        /// <param name="bufferOffset">The index with the buffer to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetChars(CurrentRowIndex, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Gets the value of the specified column as a DateTime.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override DateTime GetDateTime(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetDateTime(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a decimal.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override decimal GetDecimal(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetDecimal(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a float.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override float GetFloat(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetFloat(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a double.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override double GetDouble(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetDouble(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a Guid.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override Guid GetGuid(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetGuid(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a 16-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override short GetInt16(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetInt16(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a 32-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override int GetInt32(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetInt32(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a 64-bit signed integer.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override long GetInt64(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetInt64(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as a string.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override string GetString(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetString(CurrentRowIndex);
        }

        /// <summary>
        /// Gets the value of the specified column as an instance of <see cref="Object"/>.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the specified column.</returns>
        public override object GetValue(int ordinal)
        {
            CheckColumnIndex(ordinal);
            return _currentColumnData[ordinal].GetValue(CurrentRowIndex);
        }
    }
}
