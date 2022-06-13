# Workers

In the Open API client, all table operations occur in the context of an
<xref:Deephaven.OpenAPI.Client.IWorkerSession>.

There are two ways to get a reference to a worker session:
1. Start a new worker (this starts a new JVM on the server)
2. Attach to an already running worker (this is known as a Persistent Query).

## Worker Lifetime
@Deephaven.OpenAPI.Client.IWorkerSession
objects implement the `IDisposable` pattern. Callers should invoke `Dispose` when finished with them, or wrap
their usage in a `using` block like this:
```c#
using (var workerSession = client.StartWorker(workerOptions))
{
    ...
}
```

At disposal time, the @Deephaven.OpenAPI.Client.IWorkerSession will dispose its
@Deephaven.OpenAPI.Client.IQueryScope which will in turn dispose all of the
@Deephaven.OpenAPI.Client.IQueryTable objects that it owns exclusively (for more on this topic, see
[Resource Management with QueryScopes](./queryscopes.md)).  What happens next depends on how the
worker was created. If the Worker was initially started by this program caller, it will be stopped.
On the other hand, if this program had attached to an already-running worker (aka Persistent Query),
that worker will continue to run.

## Starting a Worker

Client code can start a worker like this:

```c#
const string host = "...";
const string user = "...";
const string password = "...";
const string operateAs = "...";

using (var client = OpenApi.Connect(host))
{
    client.Login(user, password, operateAs);
    var workerOptions = new WorkerOptions("Default");
    using (var workerSession = client.StartWorker(workerOptions))
    {
        ...
    }
}
```

Starting a new worker requires a @Deephaven.OpenAPI.Client.WorkerOptions object, which holds a variety of
worker options. Callers that want to accept the defaults can initialize the @Deephaven.OpenAPI.Client.WorkerOptions
object with the string `"Default"`, as in the example below:

```c#
const string host = "...";
const string user = "...";
const string password = "...";
const string operateAs = "...";

using (var client = OpenApi.Connect(host))
{
    client.Login(user, password, operateAs);
    var workerOptions = new WorkerOptions("Default");
    using (var workerSession = client.StartWorker(workerOptions))
    {
	    ...
    }
}
```
### Attaching to a Worker
You can attach to a Persistent Query via the
@Deephaven.OpenAPI.Client.IOpenApiClient.AttachWorkerByName* method.  In the example below, we
assume a Persistent Query name "Example PQ" is running on the server. To get a list of Persistent Queries,
you can call <xref:Deephaven.OpenAPI.Client.IOpenApiClient.GetPersistentQueryConfigs>.

```C#
using (var client = OpenApi.Connect(host))
{
    client.Login(user, password, operateAs);
    using (var workerSession = client.AttachWorkerByName("Example PQ"))
    {
    }
}
```

## What's Next

In our next chapter,
[Resource Management with QueryScopes](./queryscopes.md),
we discuss the fundamental
@Deephaven.OpenAPI.Client.IQueryScope mechanism that allows client code to arrange
server `TableHandle` resources into groups and manage their lifetimes as a group.
