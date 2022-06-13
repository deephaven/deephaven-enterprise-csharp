/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Data;

namespace Deephaven.Connector.Internal
{
    internal class TypeMapper
    {
        /// <summary>
        /// A collection of all the details needed to provide schema metadata. Most of these properties are constants at present
        /// but we represent them anyway in case we want to provide a richer data model in the future.
        /// </summary>
        public class SqlTypeInfo
        {
            public string TypeName { get; private set; }
            public int ProviderDbType { get; private set; }
            public long ColumnSize { get; private set; }
            public string CreateFormat { get; private set; }
            public string CreateParameters { get; private set; }
            public string DataType { get; private set; }
            public bool IsAutoincrementable { get; private set; }
            public bool IsBestMatch { get; private set; }
            public bool IsCaseSensitive { get; private set; }
            public bool IsFixedLength { get; private set; }
            public bool IsFixedPrecisionScale { get; private set; }
            public bool IsLong { get; private set; }
            public bool IsNullable { get; private set; }
            public bool IsSearchable { get; private set; }
            public bool IsSearchableWithLike { get; private set; }
            public bool IsUnsigned { get; private set; }
            public short MaximumScale { get; private set; }
            public short MinimumScale { get; private set; }
            public bool IsConcurrencyType { get; private set; }
            public bool IsLiteralSupported { get; private set; }
            public string LiteralPrefix { get; private set; }
            public string LiteralSuffix { get; private set; }
            public string NativeDataType { get; private set; }

            public SqlTypeInfo(SqlDbType dbType, bool signed, int precision, short scale, Type columnType, bool nullable)
            {
                ProviderDbType = (int)dbType;
                IsUnsigned = !signed;
                MaximumScale = scale;
                DataType = columnType.FullName;
                IsNullable = nullable;
            }

            public SqlTypeInfo(SqlDbType dbType, Type columnType, bool nullable)
            {
                ProviderDbType = (int)dbType;
                DataType = columnType.FullName;
                IsNullable = nullable;
            }

            public SqlTypeInfo(SqlDbType dbType, bool signed, Type columnType, bool nullable)
            {
                ProviderDbType = (int)dbType;
                IsUnsigned = !signed;
                DataType = columnType.FullName;
                IsNullable = nullable;
            }
        }

        public static SqlTypeInfo GetSqlDbType(Type columnType)
        {
            short precision = 0, scale = 0;
            if (columnType.Equals(typeof(byte)))
            {
                return new SqlTypeInfo(SqlDbType.TinyInt, true, precision, scale, typeof(byte), true);
            }
            else if(columnType.Equals(typeof(char)))
            {
                return new SqlTypeInfo(SqlDbType.Char, true, precision, scale, typeof(byte), true);
            }
            else if (columnType.Equals(typeof(Int16)))
            {
                return new SqlTypeInfo(SqlDbType.SmallInt, true, precision, scale, typeof(Int16), true);
            }
            else if (columnType.Equals(typeof(Int32)))
            {
                return new SqlTypeInfo(SqlDbType.Int, true, precision, scale, typeof(Int32), true);
            }
            else if (columnType.Equals(typeof(Int64)))
            {
                return new SqlTypeInfo(SqlDbType.BigInt, true, precision, scale, typeof(Int64), true);
            }
            else if (columnType.Equals(typeof(string)))
            {
                return new SqlTypeInfo(SqlDbType.VarChar, false, precision, scale, typeof(string), true);
            }
            else if (columnType.Equals(typeof(short)))
            {
                return new SqlTypeInfo(SqlDbType.SmallInt, true, precision, scale, typeof(short), true);
            }
            return null;
        }
    }
}
