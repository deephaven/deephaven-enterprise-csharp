/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */

namespace ConnectorTest
{
    /// <summary>
    /// This program runs various integration tests of the ADO.NET driver
    /// against a running open API server. Some queries make assumptions about
    /// what data is available (specifically, DbInternal.ProcessEvent log should
    /// have some data for the most recent days, and the ADOTestNS.ADOTestTypes
    /// table should exist for the type testing).
    /// </summary>

    // The test types table was created with a Groovy script as follows:
    /*
    // create test data
    import java.time.ZonedDateTime
    import java.time.LocalDateTime
    import java.time.ZoneId
    import com.illumon.iris.db.v2.sources.ArrayBackedColumnSource
    import com.illumon.iris.db.v2.utils.Index
    import com.illumon.iris.db.v2.QueryTable
    import java.math.BigDecimal
    import java.math.BigInteger
    import java.time.LocalDate
    import java.time.Year
    import java.time.LocalTime

    booleanData = [false, true, false, NULL_BOOLEAN, false, false] as Boolean[]
    byteData = [0, 1, -1, NULL_BYTE, Byte.MAX_VALUE, Byte.MIN_VALUE + 1] as byte[]
    shortData = [0, 1, -1, NULL_SHORT, Short.MAX_VALUE, Short.MIN_VALUE + 1] as short[]
    intData = [0, 1, -1, NULL_INT, Integer.MAX_VALUE, Integer.MIN_VALUE + 1] as int[]
    longData = [0, 1, -1, NULL_LONG, Long.MAX_VALUE, Long.MIN_VALUE + 1] as long[]
    charData = ['A', 'B', 'C', NULL_CHAR, Character.MAX_VALUE, Character.MIN_VALUE] as char[]
    floatData = [0.0, 1.0, -1.0, NULL_FLOAT, Float.MAX_VALUE, -3.4e+38f] as float[]
    doubleData = [0.0, 1.0, -1.0, NULL_DOUBLE, Double.MAX_VALUE, -1.79e+308] as double[]
    stringData = ["Hello", "World", "!", null, "Some", "Strings"] as String[]
    dbDateTimeData = [
            DBTimeUtils.toDateTime(ZonedDateTime.of(LocalDateTime.of(2019, 1, 1, 12, 0, 0), ZoneId.of("UTC"))),
            DBTimeUtils.toDateTime(ZonedDateTime.of(LocalDateTime.of(2019, 3, 11, 12, 59, 59), ZoneId.of("UTC"))),,
            null,
            null,
            null,
            null
    ] as DBDateTime[]
    bigDecimalData = [
            BigDecimal.valueOf(0.0d),
            BigDecimal.valueOf(1.0d),
            BigDecimal.valueOf(-1.0d),
            null,
            new BigDecimal("79228162514264337593543950335"),
            new BigDecimal("-79228162514264337593543950335")
    ] as BigDecimal[]
    bigIntegerData = [
            BigInteger.valueOf(0L),
            BigInteger.valueOf(1L),
            BigInteger.valueOf(-1L),
            null,
            BigInteger.valueOf(Long.MAX_VALUE),
            BigInteger.valueOf(Long.MIN_VALUE)
    ] as BigInteger[]
    localDateData = [
            LocalDate.of(2019, 1, 1),
            LocalDate.of(2019, 12, 31),
            null,
            null,
            LocalDate.of(9999, 12, 31),
            LocalDate.of(1, 1, 1)
    ] as LocalDate[]
    localTimeData = [
            LocalTime.of(0, 0, 0, 0),
            LocalTime.of(1, 0, 0, 0),
            LocalTime.of(13, 59, 59, 0),
            null,
            LocalTime.of(23, 59, 59, 999999999),
            LocalTime.of(0, 0, 0, 0)
    ] as LocalTime[]


    Double.MAX_VALUE
    columns = [
            'BooleanColumn':ArrayBackedColumnSource.getMemoryColumnSource(booleanData),
            'ByteColumn':ArrayBackedColumnSource.getMemoryColumnSource(byteData),
            'ShortColumn':ArrayBackedColumnSource.getMemoryColumnSource(shortData),
            'IntColumn':ArrayBackedColumnSource.getMemoryColumnSource(intData),
            'LongColumn':ArrayBackedColumnSource.getMemoryColumnSource(longData),
            'CharColumn':ArrayBackedColumnSource.getMemoryColumnSource(charData),
            'FloatColumn':ArrayBackedColumnSource.getMemoryColumnSource(floatData),
            'DoubleColumn':ArrayBackedColumnSource.getMemoryColumnSource(doubleData),
            'StringColumn':ArrayBackedColumnSource.getMemoryColumnSource(stringData),
            'DateTimeColumn':ArrayBackedColumnSource.getMemoryColumnSource(dbDateTimeData),
            'BigDecimalColumn':ArrayBackedColumnSource.getMemoryColumnSource(bigDecimalData),
            'BigIntegerColumn':ArrayBackedColumnSource.getMemoryColumnSource(bigIntegerData),
            'LocalDateColumn':ArrayBackedColumnSource.getMemoryColumnSource(localDateData),
            'LocalTimeColumn':ArrayBackedColumnSource.getMemoryColumnSource(localTimeData),
    ]

    index = Index.FACTORY.getFlatIndex(doubleData.length)

    table = new QueryTable(index, columns)

    db.addTable("ADOTestNS","ADOTestTypes", table)
    */

    class Program
    {
        static void Main(string[] args)
        {
            string host = "openapi-bhs.int.illumon.com", username = "iris", password = "iris";

            string connectionString = string.Format(
                "Host={0};Username={1};Password={2}", host, username, password);

            if (args.Length > 0)
                host = args[0];
            if (args.Length > 1)
                username = args[1];
            if (args.Length > 2)
                password = args[2];

            new SimpleDbInternalQuery(connectionString).Run();

            new TestTypesQuery(connectionString).Run();

            TestOptionsQuery testOptionsQuery = new TestOptionsQuery(host, username, password);
            testOptionsQuery.RunWithBasicOptions();
            testOptionsQuery.RunWithFetchSize(10);
            testOptionsQuery.RunWithOperateAs("iris");
            testOptionsQuery.RunWithMaxHeap(1024);
            testOptionsQuery.RunWithSessionType(Deephaven.Connector.SessionType.Python);
            testOptionsQuery.RunWithDebugOption(8890, false);

            new TestSimpleBoundParameterQuery(connectionString).Run();

            new TestBoundTypesQuery(connectionString).Run();
            new TestBoundTypesQuery(connectionString).RunWithPython();

            new TestWhiteListQuery(connectionString).Run();

            new MultiQueryTest(connectionString).Run();
        }
    }
}
