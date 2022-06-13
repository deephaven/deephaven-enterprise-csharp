## Fetch Size

Fetch size is an important parameter when querying large tables. The default fetch size is 10,000 rows. This can be overridden by setting the FetchSize property on the DeephavenDataReader object prior to calling Read, as in the example below. Unfortunately there is no standard interface for setting fetch size on DbDataReader, so a cast is necessary.

Larger fetch sizes may improve throughput on large tables, but will also require more memory and increase latency between each fetch (as that "chunk" of data is sent over the network).

```C#
using (DeephavenConnection connection = new DeephavenConnection("Host=<openapi-host>;Username=<username>;Password=<password>"))
{
  connection.Open();
  using (DbCommand command = connection.CreateCommand())
  {
    command.CommandText = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                + ".where(\"Date = currentDateNy()\")"
                + ".head(100)";
    using (DbDataReader reader = command.ExecuteReader())
    {
      ((DeephavenDataReader)reader).FetchSize = 1000;
      // ...
    }
  }
}
```