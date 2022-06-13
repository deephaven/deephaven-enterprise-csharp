# Asynchronous Programming

Many of the entry points in the Open API interface have both a synchronous and asynchronous
form. The advantage of the synchronous form is that tends to be easier to code against.
On the other hand, the advantage of the asynchronous form is that it allows for more
parallelism and potentially better performance.

We express asynchrony with the .Net `Task` framework. In general, when we have a void or
value-returning synchronous method that look like this:
```c#
void DoOperation1(args...)
T DoOperation2(args...)
```

...the corresponding asynchronous methods will look like this:
```c#
Task DoOperation1Task(args...)
Task<T> DoOperation2(args...)
```

As a matter of fact, in cases where the asynchronous method exists, the synchronous method
always simply delegates to it:
```c#
void DoOperation1(args...)
{
  DoOperation1Task(args...).Wait();
}

T DoOperation2(args...)
{
    return ExceptionUtil.ResultOrUnwrappedException(
        DoOperation2Task(args...));
}
```

The `ResultOrUnwrappedException` method is a minor
implementation detail: it is a simple helper method that unwraps `AggregateExceptions`
and throws their inner exception instead.

Interested readers should consult the .NET documentation for detailed information about the .NET
Task Framework. Here we would only like to stress three important considerations:

## Always `Wait` or observe `Result` on your Tasks; don't fire-and-forget

The Task Framework is an asynchronous programming framework, not a threading framework per se (even
though Tasks often do run on separate threads). Callers should not simply create Tasks and assume
they will run to completion on their own. Instead, callers they should be careful to always call
`.Wait` or `.Result` on Tasks that they are responsible for.  If they fail to do so, the Task code
might not finish executing or a thrown exception might not properly propagate.

## Prefer async/await

The async/await pattern makes it very easy and natural to write code that properly chains Tasks,
propagate exceptions, and so forth. Once you get used to it, it tends to be a lot easier to write
code as opposed to chaining callbacks e.g. with `.ContinueWith`.

## Create asynchronous TaskCompletionSources

For the sake of efficiency, if one piece of async code is waiting on a `TaskCompletionSource`
result, and another piece of code calls `SetResult`, the setting code may invoke the waiting callback
immediately (on its own thread), rather than dispatching it on another thread. This can lead to some
very deep stack traces and even deadlocks in Task-heavy code, because it can introduce dependencies
between two pieces of code that otherwise appear independent. The simple rule of thumb is if you do use
`TaskCompletionSource`, always make it asynchronous:

```
var tcs = new TaskCompletionSource<T>(
    TaskCreationOptions.RunContinuationsAsynchronously);
```

## Example

The following (somewhat artificial) example spawns two Workers in parallel, performs a query on each
Worker, and reports the number of rows returned by each query.

```c#
public static void TaskExample()
{
    using (var client = OpenApi.Connect(host))
    {
        client.Login(user, password, operateAs);
        var ticker0 = "AAPL";
        var ticker1 = "ZNGA";

        var task0 = MakeWorkerAndFetchTable(client, ticker0);
        var task1 = MakeWorkerAndFetchTable(client, ticker1);
        var sizes = Task.WhenAll(task0, task1).Result;

        Console.WriteLine(
	    $"Tasks are finished: {ticker0} had {sizes[0]} rows, and {ticker1} had {sizes[1]} rows.");
    }
}

// Note async keyword
private static async Task<long> MakeWorkerAndFetchTable(IOpenApiClient client, string tickerToFind)
{
    var workerOptions = new WorkerOptions("Default");
    DebugUtil.Print($"[{tickerToFind}]: Starting worker");
    // Note: we are using the await keyword
    using (var workerSession = await client.StartWorkerTask(workerOptions))
    {
        var scope = workerSession.QueryScope;

        DebugUtil.Print($"[{tickerToFind}]: Getting historical table");
        var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");

        DebugUtil.Print($"[{tickerToFind}]: Getting columns");
        // note await keyword
        var (importDate, ticker) = await table.GetColumnsTask<StrCol, StrCol>("ImportDate", "Ticker");
        var filtered = table.Where(importDate == "2017-11-01" &&
	    ticker == tickerToFind);

        DebugUtil.Print($"[{tickerToFind}]: Getting table data");
        // note await keyword
        var tableData = await filtered.GetTableDataTask();
        return tableData.Rows;
    }
}
```

Note: `DebugUtil.Print` is our method for printing a message to the console along
with its thread id. One run of the sample program looks like this:
```
[1]: [AAPL]: Starting worker
[1]: [ZNGA]: Starting worker
[6]: [AAPL]: Getting historical table
[6]: [AAPL]: Getting columns
[10]: [AAPL]: Getting table data
[7]: [ZNGA]: Getting historical table
[7]: [ZNGA]: Getting columns
[10]: [ZNGA]: Getting table data
```

Note that various pieces of the asynchronous code ended up running on various threads in the thread pool.

## What's Next

In our next chapter,
[Table Operations](./operations.md),
we describe the various table operations that are possible.
