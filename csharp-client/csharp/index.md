# Deephaven .NET API

## Introduction
The Deephaven .NET libraries provide two ways of interacting with the system: the
[Deephaven ADO.NET Data Provider](./doc_ado/intro.md) (the "Connector")
and the
[Deephaven Open API client](./doc_client/intro.md).

Which API is best for you depends on the application. The ADO.NET Connector is typically used for:
* importing Deephaven data into existing tools that consume ADO.NET/SQL data sources,
* use cases where only table snapshots are required,
* and/or situations where the developer is already accustomed to using ADO.NET result sets.

On the other hand, if you want to consume dynamic ("ticking") data, or need a richer client-side API
for constructing and composing Deephaven queries, the Open API interface is probably more appropriate.

See the following chapters for more:
* [Deephaven ADO.NET Data Provider](./doc_ado/intro.md)
* [Deephaven Open API client](./doc_client/intro.md)
* [Deephaven API documentation](./api/index.md)
