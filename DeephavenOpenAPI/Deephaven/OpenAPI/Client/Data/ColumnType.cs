/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace Deephaven.OpenAPI.Client.Data
{
    /// <summary>
    /// This enumeration represents the set of legal column types in the
    /// Open API. This is a limited subset of column types available in Deephaven.
    /// </summary>
    public enum ColumnType
    {
        Int,
        Long,
        Short,
        Byte,
        Char,
        Float,
        Double,
        Decimal,
        BigInteger,
        String,
        DateTime,
        LocalDate,
        LocalTime
    }
}
