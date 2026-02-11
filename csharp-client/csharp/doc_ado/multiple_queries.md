## Multiple Queries
The Deephaven DbDataReader implementation supports the reading of multiple results. This may be advantageous to avoid additional round-trips to the server. Each query expression must be delimited with a newline (in Python or Groovy). The standard NextResult method is used to iterate the results as in this example:

``` C#
using (DeephavenConnection connection = new DeephavenConnection("Host=<openapi-host>;Username=<username>;Password=<password>"))
{
  connection.Open();
  using (DbCommand command = connection.CreateCommand())
  {
    var query1 = db.i(\"DbInternal\",\"ProcessEventLog\")"
                + ".where(\"Date = currentDateNy()\")"
                + ".head(100)";
    var query2 = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                + ".where(\"Date = currentDateNy()\")"
                + ".head(100)";
    var query3 = "db.i(\"DbInternal\",\"ProcessEventLog\")"
                + ".where(\"Date = currentDateNy()\")"
                + ".head(100)";
    command.CommandText = query1 + "\n" + query2 + "\n" + query3;
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
      if(reader.NextResult())
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
        if(reader.NextResult())
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
  }
}
```