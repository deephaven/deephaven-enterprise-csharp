/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Deephaven.OpenAPI.Shared.Data;
using Deephaven.OpenAPI.Shared.Data.Columns;
using Deephaven.OpenAPI.Client.Data;

using SharedColumnData = Deephaven.OpenAPI.Shared.Data.Columns.ColumnData;
using ClientColumnData = Deephaven.OpenAPI.Client.Data.IColumnData;

namespace Deephaven.OpenAPI.Client.Internal
{
    /// <summary>
    /// Internal utility for creating ColumnData objects from incoming Open API data.
    /// </summary>
    internal static class ColumnDataFactory
    {
        private static readonly Visitor _visitor = new Visitor();

        /// <summary>
        /// Wrap a low level column data object with our stronger-typed version.
        /// </summary>
        internal static ClientColumnData WrapColumnData(ColumnDefinition columnDefinition,
            SharedColumnData columnData)
        {
            return columnData?.AcceptVisitor(_visitor, columnDefinition);
        }

        internal static ClientColumnData[] WrapColumnData(ColumnDefinition[] columnDefinition,
            SharedColumnData[] columnData)
        {
            return columnDefinition.Zip(columnData, WrapColumnData).ToArray();
        }

        private class Visitor : IColumnDataVisitor<ColumnDefinition, ClientColumnData>
        {
            public ClientColumnData Visit(BigDecimalArrayColumnData self, ColumnDefinition def)
            {
                return new DecimalColumnData(self);
            }

            public ClientColumnData Visit(BigIntegerArrayColumnData self, ColumnDefinition def)
            {
                return new BigIntegerColumnData(self);
            }

            public ClientColumnData Visit(ByteArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(ByteArrayColumnData self, ColumnDefinition def)
            {
                return def.Type == "java.lang.Boolean" ?
                    (ClientColumnData)new BooleanColumnData(self) :
                    new ByteColumnData(self);
            }

            public ClientColumnData Visit(CharArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(CharArrayColumnData self, ColumnDefinition def)
            {
                return new CharColumnData(self);
            }

            public ClientColumnData Visit(DoubleArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(DoubleArrayColumnData self, ColumnDefinition def)
            {
                return new DoubleColumnData(self);
            }

            public ClientColumnData Visit(FloatArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(FloatArrayColumnData self, ColumnDefinition def)
            {
                return new FloatColumnData(self);
            }

            public ClientColumnData Visit(IntArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(IntArrayColumnData self, ColumnDefinition def)
            {
                return new IntColumnData(self);
            }

            public ClientColumnData Visit(LocalDateArrayColumnData self, ColumnDefinition def)
            {
                return new DateColumnData(self);
            }

            public ClientColumnData Visit(LocalTimeArrayColumnData self, ColumnDefinition def)
            {
                return new TimeColumnData(self);
            }

            public ClientColumnData Visit(LongArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(LongArrayColumnData self, ColumnDefinition def)
            {
                return def.Type == "com.illumon.iris.db.tables.utils.DBDateTime"
                    ? (ClientColumnData) new DBDateTimeColumnData(self)
                    : new LongColumnData(self);
            }

            public ClientColumnData Visit(ShortArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(ShortArrayColumnData self, ColumnDefinition def)
            {
                return new ShortColumnData(self);
            }

            public ClientColumnData Visit(StringArrayArrayColumnData self, ColumnDefinition def)
            {
                return NotImplemented(self);
            }

            public ClientColumnData Visit(StringArrayColumnData self, ColumnDefinition def)
            {
                return new StringColumnData(self);
            }

            /// <summary>
            /// Convenience method for formatting an exception message.
            /// </summary>
            /// <returns>Never returns anything but has a return type for notational convenience.</returns>
            private ClientColumnData NotImplemented(SharedColumnData self)
            {
                throw new ArgumentException($"Unsupported column data type: {self.GetType().Name}");
            }
        }
    }
}
