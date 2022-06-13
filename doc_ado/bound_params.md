## Bound Parameters
Query parameters can be bound with a syntax similar to many SQL implementations, using the "@" character prefix, as the following example demonstrates. All bound parameters must be prefixed with the "@" character and the query processor will expect a matching bound parameter for each variable prefixed as such. In order to include a literal "@" character, simply double it as shown in the updateView expression below.

``` C#
using (DeephavenConnection connection = new DeephavenConnection("Host=<openapi-host>;Username=<username>;Password=<password>"))
{
  connection.Open();
  using (DbCommand command = connection.CreateCommand())
  {
    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                + ".where(\"Date = @date\")"
                + ".head(100).updateView(\"SomeEmail=`fake@somewhere.com`\")";
    DbParameter dateParam = command.CreateParameter();
    dateParam.DbType = System.Data.DbType.String;
    dateParam.Value = "2019-08-13";
    dateParam.ParameterName = "@date";
    command.Parameters.Add(dateParam);     
    using (DbDataReader reader = command.ExecuteReader())
    {
      while (reader.Read())
      {
        for (var i = 0; i < reader.FieldCount; i++)
        {
          Console.Write("\t");
          Console.Write(reader[i]);
        }
        Console.WriteLine();
      }
    }
  }
}
```

###Bound Types
The supported types for bound parameters are very similar to the supported column types, with a few exceptions. There is no way to bind BigInteger variables at present. The following table shows what Deephaven type each DbType will map to in the Deephaven query, and what types may be assigned to the Value of the DbParameter object.

| DbType | Deephaven Type | Legal Bind Types |
| ------ | -------------- | ---------------- |
| Boolean | Boolean | bool |
| SByte | byte | Byte, sbyte |
| Byte | byte | Byte, sbyte |
| Int16 | short | Byte, Int16 |
| Int32 | int | Byte, Int16, Int32 |
| Int64 | long | Byte, Int16, Int32, Int64 |
| Single | float | Float, Byte, Int16, Int32, Int64 |
| Double | double | Double, Single, Byte, Int16, Int32, Int64 |
| Decimal | BigDecimal | Decimal, string* |
| DateTime | DBDateTime | DateTime, Int64 (nanos) |
| Date | LocalDate | DateTime |
| Time | LocalTime | DateTime |
* Numbers larger than those representable by decimal can be specified using a string
