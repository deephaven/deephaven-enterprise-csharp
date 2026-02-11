# Deephaven Open API Client

The Deephaven Open API Client Library provides a high-level interface to Open API functionality.
The library...
* Provides a "fluent" syntax which provides a more convenient and less error-prone way of expressing
  expressions and conditionals, with much of the syntax checking being done at compile time.
* Gives client code choice of either synchronous or asynchronous method invocations, via the .NET
  Task Framework.
* Provides an event interface to allow client code to handle asynchronous table events like ticking
  tables.
* Allows for deterministic cleanup of server resources via the
  <xref:Deephaven.OpenAPI.Client.IQueryScope>
  framework as well as the .NET `Dispose` pattern.
* Allows the client to create new Deephaven workers or connect to existing workers (e.g. persistent
  queries).

## Hello, World

The following example code connects to a Deephaven instance, logs in, starts a worker and executes
a query on a
historical table.

```c#
public static void HelloWorld()
{
    const string host = "...";
    const string user = "...";
    const string password = "...";

    using (var client = OpenApi.Connect(host))
    {
        client.Login(user, password, null);
        var workerOptions = new WorkerOptions("Default");
        using (var workerSession = client.StartWorker(workerOptions))
        {
            var scope = workerSession.QueryScope;
            var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
            var (importDate, ticker) = table.GetColumns<StrCol, StrCol>(
	        "ImportDate", "Ticker");
            var filtered = table.Where(
	        importDate == "2017-11-01" && ticker == "AAPL");
            PrintUtils.PrintTableData(filtered);
        }
    }
}
```

Open API programs typically follow the above recipe:
* Connect to the server with <xref:Deephaven.OpenAPI.Client.OpenApi.Connect*>
  providing the server hostname and
  optional port and event callback arguments.
* <xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.Login*>
  with username/password or publickey/privatekey credentials.
* Create or attach to a worker with
<xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.StartWorker*>,
<xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.AttachWorkerByName*>, or
<xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.AttachWorkerBySerial*>.
* Perform queries using either the main scope from the worker session or other sub-scopes
  that it creates. For a detailed discussion of scopes, see [Resource Management with QueryScopes](./queryscopes.md).
* Clean up resources, either explicitly with `Dispose`, or implicitly, as in the example above,
  with <xref:Deephaven.OpenAPI.Client.IQueryScope> and `using`.

## Hello, World with Key-Based Authentication

It's considered a best practice to avoid embedding passwords in source code. The preferred
alternative is key-based authentication. To use this, ask your administrator to generate your
credentials file, using
[the instructions found here](https://docs.deephaven.io/latest/Content/User/advancedTopics/DBAuthentication.htm?Highlight=generate-iris-key).
Then, copy the file priv-*myuser*-iris.base64.txt to a secure place on the client file system. Once this
file is in place, you can replace the `client.Login(...)` line in the above example with code like
```
client.Login("/home/myuser/mykeys/priv-myuser.base64.txt");
```

Security note: keep this file in a secure place on your file system (perhaps protected by file permissions).
Anyone who can access this file will be able to log into Deephaven using those credentials.

## What's Next

In our first chapter,
[Workers](./workers.md),
we discuss how to create a new Deephaven Worker or attach to an existing one.
