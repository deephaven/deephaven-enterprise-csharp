## Connection Options
The following options are legal in the connection string, or may be specified using the type safe DeephavenConnectionStringBuilder object.

| Property | Type | Default | Description |
| -------- | ---- | ------- | ----------- |
| Host | string | n/a | The host or IP address of the Deephaven OpenAPI server. |
| Port | int | 8123 | The TCP/IP port of the OpenAPI server. |
| Username | string | n/a | The username to connect with. |
| Password | string | n/a | The password to connect with. |
| OperateAs | string | n/a | The username to operate-as. |
| RemoteDebugPort | int | -1 | Remote session debugging port. Negative values indicate no remote debugging. |
| SuspendWorker | bool | n/a |Whether the worker process should suspend and wait for a debugger on startup (only applicable if RemoteDebugPort > 0). |
| MaxHeapMb | int | 2048 | Maximum heap size in MB. |
| TimeoutMs | int | 30,000 | Connection timeout in milliseconds. |
| SessionType | SessionType | Groovy | The session type (Groovy/Python). |
| LocalDateAsString | bool | false | Whether to return LocalDate column data as strings instead of DateTime. |
| LocalTimeAsString | bool | false | Whether to return LocalTime column data as strings instead of DateTime. |
