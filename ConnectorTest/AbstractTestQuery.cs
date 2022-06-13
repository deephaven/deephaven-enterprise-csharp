/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;
using System.Data.Common;

namespace ConnectorTest
{
    public class AbstractTestQuery
    {
        protected void TestValueMatches<T>(DbDataReader dbDataReader, int row, int ordinal, T expectedValue, params Func<int, T>[] getters)
        {
            if (dbDataReader.IsDBNull(ordinal))
            {
                // make sure we were expecting null
                if (!Object.Equals(expectedValue, null))
                {
                    throw new ArgumentException(string.Format("Expected value for column {0}, row {1} to be {2} but it was null instead",
                        ordinal, row, expectedValue));
                }
                // if IsDBNull returns true, we also expect the accessor to return DBNull.Value
                if (dbDataReader[ordinal] != DBNull.Value)
                {
                    throw new ArgumentException(string.Format("Expected object value for column {0}, row {1} to be DBNull.Value but it was {2} instead",
                        ordinal, row, dbDataReader[ordinal]));
                }
            }
            else
            {
                // make sure the [] operator works as expected
                T value = (T)dbDataReader[ordinal];
                if (!value.Equals(expectedValue))
                {
                    throw new ArgumentException(string.Format("Expected accessor value for column {0}, row {1} to be {2} but it was {3} instead",
                            ordinal, row, expectedValue, dbDataReader[ordinal]));
                }
                // make sure the matching getter (ie GetBoolean(), GetInt32()) works as expected
                foreach (var getter in getters)
                {
                    T value2 = getter.Invoke(ordinal);
                    if (!value2.Equals(expectedValue))
                    {
                        throw new ArgumentException(string.Format("Expected getter value for column {0}, row {1} to be {2} but it was {3} instead",
                                ordinal, row, expectedValue, dbDataReader[ordinal]));
                    }
                }
            }
        }

        protected void AddDbParameter(DbCommand dbCommand, DbType dbType, string name, object value)
        {
            DbParameter parameter = dbCommand.CreateParameter();
            parameter.DbType = dbType;
            parameter.ParameterName = name;
            parameter.Value = value;
            dbCommand.Parameters.Add(parameter);
        }
    }
}
