## Types

A subset of Deephaven column types are fully supported. All others (for example arbitrary java types) will work, but appear as string columns. Where possible, we attempt to support "getter" methods on DbReader that are not lossy (i.e. one can retrieve an int value with GetLong but not vice-versa). All types are nullable, and IsDBNull should return appropriately (note the comments regarding special values used to represent null for numeric types in Deephaven).

The following table enumerates the getter methods implemented for each Deephaven column type. All column types also support the GetValue method, which returns either null or the appropriate value according to the default getter.

| Deephaven/Java Type | DbDataReader getters | Comments |
| ------------------- | -------------------- | -------- |
| Boolean| *GetBoolean | GetBoolean will return false for null values, but null and false can be differentiated using the IsDBNull method. |
| byte | *GetByte GetInt16 GetInt32 GetInt64 | Deephaven byte columns are signed, while .NET byte values are unsigned, so using GetByte() on the data reader will result in a cast. Using GetInt16/32/64 will preserve the original value. Also Deephaven byte columns use the value -128 to represent null, so the effective range of these columns is -127 to +127. |
| short | *GetInt16 GetInt32 GetInt64 | Deephaven short columns use the minimum value to represent null values, so the effective range of these columns excludes that value. |
| int | *GetInt32 GetInt64 | Deephaven int columns use the minimum value to represent null values, so the effective range of these columns excludes that value. |
| long | *GetInt64 | Deephaven long columns use the minimum value to represent null values, so the effective range of these columns excludes that value. |
| float | *GetSingle GetDouble | Deephaven long columns use the value -Single.MaxValue to represent null values, so the effective range of these columns excludes that value. |
| double | *GetDouble | Deephaven long columns use the value -Double.MaxValue to represent null values, so the effective range of these columns excludes that value. |
| BigDecimal | *GetDecimal GetString | Unlike .NET decimal, Deephaven BigDecimal columns have unlimited precision, so it is possible to generate overflow exceptions with very large values. In that case, the GetString getter can retrieve a simple string representation of the value (no commas or localization other than using the local decimal separator character). |
| BigInteger | *GetDecimal GetString | BigInteger columns are treated in exactly the same way as BigDecimal. |
| DBDateTime | *GetDateTime GetInt64 GetString | Internally, Deephaven DBDateTime values are represented as nanoseconds-since-the-epoch. This value can be retrieved directly using the GetInt64 method. The GetDateTime getter will return a UTC DateTime truncated to the nearest millisecond. |
| LocalDate | *GetDate GetString | The range of the java/Deephaven LocalDate type is larger than that of the .NET DateTime, so using GetDate may generate an exception. To avoid this risk, use GetString, which return a string formatted as <year>-<month>-<day>. |
| LocalTime | *GetTime GetString | java.Deephaven LocalTime columns have nanosecond precision. GetTime will return a UTC DateTime where the date portion is the start-of-the-epoch and the time of day is truncated to the millisecond. GetString will return a string formatted as <hour>:<minute>:<second>.<nanosecond>. |
| String | *GetString | |

* The default/recommended method, equivalent to GetValue on this column type for non-null values.
