# Deephaven ADO.NET Data Provider
This API allows you to fetch Deephaven table snapshots using a standard ADO.NET interface. Because Deephaven does not presently have SQL support, only Groovy and Python query expressions are supported. Only the query portion of the ADO.NET API is implemented; equivalents to SQL INSERT/UPDATE/DELETE statements are not supported, nor are DDL statements.

Since Deephaven tables may "tick", the driver takes a snapshot of the table produced by the provided query and returns rows from this snapshot incrementally (in chunks according to a "fetch size"). In this way, large tables can be queried consistently. If dynamic access to live tables and ticking data are desired, use the Open API directly.

## Simple Example

```C#
var builder = new DeephavenConnectionStringBuilder();
builder.Host = ...;
builder.Username = ...;
builder.Password = ...;

using (var connection = new DeephavenConnection(builder))
{
    connection.Open();
    using (var command = connection.CreateCommand())
    {
        command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                              + ".where(\"Date = currentDateNy()\")"
                              + ".head(100)";
        using (var reader = command.ExecuteReader())
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
