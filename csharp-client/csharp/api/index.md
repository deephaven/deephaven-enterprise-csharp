# Deephaven .NET API
The documentation here describes two packages - a high-level Open API Client and the Deephaven ADO.NET Data Provider.
Both of these connect to Deephaven via the low-level Open API protocol.

## ADO.NET Data Provider
The primary classes relevant to the ADO.NET Data Provider are as follows.

<xref:Deephaven.Connector.DeephavenConnection>
Implements the IDbConnection interface as an Open API client.

<xref:Deephaven.Connector.DeephavenCommand>
Implements the IDbCommand interface such that Deephaven queries can be executed over Open API.

<xref:Deephaven.Connector.DeephavenDataReader>
Provides a snapshot of Deephaven table data implemented as a IDataReader.

<xref:Deephaven.Connector.DeephavenParameter>
Represents a bound query parameter.

<xref:Deephaven.Connector.DeephavenParameterCollection>
A collection of bound query parameters relevant to a <xref:Deephaven.Connector.DeephavenCommand>

<xref:Deephaven.Connector.DeephavenConnectionStringBuilder>
Represents a strongly typed connection string builder for Deephaven connections.

<xref:Deephaven.Connector.DeephavenProviderFactory>
Represents a set of methods for creating instances of Deephaven's implementation of the data source classes.

## Open API Client
The primary interfaces to the Open API client are as follows.

<xref:Deephaven.OpenAPI.Client.OpenApi>
Provides an static interface for the creation of new <xref:Deephaven.OpenAPI.Client.IOpenApiClient> instances.

<xref:Deephaven.OpenAPI.Client.IOpenApiClient>
Represents a connection to a Deephaven instance. Provides methods for logging-in to the server and creating
<xref:Deephaven.OpenAPI.Client.IWorkerSession> instances.

<xref:Deephaven.OpenAPI.Client.IWorkerSession>
Represents a connection to a Deephaven worker process. Provides methods for retrieving tables from the server, including intraday and 
historical tables, time tables, in-memory temporary tables, and tables bound to a variable (in an attached worker
session). These are provided as instances of <xref:Deephaven.OpenAPI.Client.IQueryTable>

<xref:Deephaven.OpenAPI.Client.IQueryTable>
An interface for querying a table, including sorts, filters and other table operations. Query table objects can be
chained arbitrarily, initially exist only on the client and will be resolved on the server only when necessary or
specifically requested.
