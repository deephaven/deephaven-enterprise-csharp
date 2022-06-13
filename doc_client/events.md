# Events

The Open API interface allows the client to subscribe to a variety of asynchronous events. These
events fall in the following categories:
* Server events: server closed, server errors, events related to Persistent Queries
* Worker events: workers opened, workers closed, worker errors, log messages
* Table events: table update events and table snapshot events

## Server Events

The client subscribes to server events by implementing the
<xref:Deephaven.OpenAPI.Client.IOpenApiListener> interface and passing that object to
<xref:Deephaven.OpenAPI.Client.OpenApi.Connect*>. For the sake of implementation convenience we also
provide the class <xref:Deephaven.OpenAPI.Client.AbstractOpenApiListener> which provides default
(no-op) implementations of all the interface methods; this frees the caller from having to implement
every method in the interface: it can implement only the methods it cares about. We also provide
the class <xref:Deephaven.OpenAPI.Client.OpenApiEventDispatcher> for callers who prefer to use
event rather than interface syntax.

## Worker Events

The client subscribes to worker events by implementing the
<xref:Deephaven.OpenAPI.Client.IWorkerListener> interface and passing that object to the
<xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.StartWorker*>,
<xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.AttachWorkerByName*>, or
<xref:Deephaven.OpenAPI.Client.OpenApiClient_Extensions.AttachWorkerBySerial*> methods.  For the sake of
implementation convenience we also provide the class
<xref:Deephaven.OpenAPI.Client.AbstractWorkerListener> which provides default (no-op)
implementations of all the interface methods; this frees the caller from having to implement every
method in the interface: it can implement only the methods it cares about. We also provide the class
<xref:Deephaven.OpenAPI.Client.WorkerEventDispatcher> for callers who prefer to use event rather
than interface syntax.

## Table Update Events

*Note:* This interface is in flux and is subject to change. It currently exposes a low-level
Deephaven protocol object to the client, `Deephaven.OpenAPI.Shared.Data.DeltaUpdates`. It is our
intention to update this interface so it uses a higher-level object instead.

The client subscribes to table update events by attaching a callback to the
<xref:Deephaven.OpenAPI.Client.IQueryTable.OnTableUpdate> event, for example with a lambda:
```
table.OnTableUpdate += (table, update) =>
{
    ...
};
```

`table` is of type <xref:Deephaven.OpenAPI.Client.IQueryTable> and refers to the IQueryTable that is firing the event.
`udpate` is of type <xref:Deephaven.OpenAPI.Client.ITableUpdate>.
It currently contains the single getter <xref:Deephaven.OpenAPI.Client.ITableUpdate.DeltaUpdates*> which is
of type `Deephaven.OpenAPI.Shared.Data.DeltaUpdates`. As discussed above, this is currently
a low-level protocol object. In a future version of the client, we will return a higher-level
object instead.

## Table Snapshot Events

The client subscribes to table snapshot events by attaching a callback to the
<xref:Deephaven.OpenAPI.Client.IQueryTable.OnTableSnapshot> event, for example with a lambda:
```
table.OnTableSnapshot += (table, snapshot) =>
{
    ...
};
```

`table` is of type <xref:Deephaven.OpenAPI.Client.IQueryTable> and refers to the IQueryTable that is firing the event.
`snapshot` is of type <xref:Deephaven.OpenAPI.Client.ITableSnapshot>. The definition of each of the properties
of <xref:Deephaven.OpenAPI.Client.ITableSnapshot> is provided in its API documentation.

## What's Next

In the next chapter, we cover [Asynchronous Programming](./async.md).
