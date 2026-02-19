# Resource Management with QueryScopes

Resources on the client (connections, worker sessions, and table references) are linked to
corresponding resources on the server. In particular, a client program may create a lot of table
references over its lifetime. A well-behaved client program should dispose of its resources whenever
it is finished using them, so the corresponding server resources can be freed as well.

Consider the following code:

```c#
using (var client = OpenApi.Connect(host))
{
    client.Login(user, password, operateAs);
    var workerOptions = new WorkerOptions("Default");
    using (var workerSession = client.StartWorker(workerOptions))
    {
        var scope = workerSession.QueryScope;
        var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
        var (importDate, ticker, close, volume) =
            table.GetColumns<StrCol, StrCol, NumCol, NumCol>(
            "ImportDate", "Ticker", "Close", "Volume");
        var filtered = table
            .Where(importDate == "2017-11-01" && ticker.StartsWith("K"))
            .Select(ticker, close, volume)
            .Sort(volume.Descending())
            .Head(10);
        PrintUtils.PrintTableData(filtered);
    }
}
```

Thanks to the `using` statements that initialized `client` and `workerSession`, the resources
managed by those two variables will be cleaned up on exit (whether normal or abnormal) from the
corresponding `using` blocks. On the other hand, there are five table references (two explicit and
three implicit) created in the above code, and the reader may wonder whether those resources are
being correctly disposed. The answer is yes: they are managed and cleaned up by the
@Deephaven.OpenAPI.Client.IQueryScope pattern. In the next section, we describe two other approaches to resource disposal, and then
we describe our preferred approach, the @Deephaven.OpenAPI.Client.IQueryScope pattern.

## Approaches to Resource Disposal

There are five table references in the above example:
1. `table` (created by <xref:Deephaven.OpenAPI.Client.IQueryScope.HistoricalTable*>)
2. An anonymous compiler temporary, not assigned to any variable, created by calling .<xref:Deephaven.OpenAPI.Client.IQueryTable.Where*> on the
   above.
3. Another anonymous compiler temporary, created by calling .<xref:Deephaven.OpenAPI.Client.IQueryTable.Select*> on the above.
4. Another anonymous compiler temporary, created by calling .<xref:Deephaven.OpenAPI.Client.IQueryTable.Sort*> on the above.
5. `filtered`, created by calling .<xref:Deephaven.OpenAPI.Client.IQueryTable.Head*>  on the above.

The Open API Client has three different strategies for disposing of table references:
* The garbage collector
* The .NET `Dispose` pattern (explicitly or via `using`)
* The Open API concept of @Deephaven.OpenAPI.Client.IQueryScope

### Resource disposal via the garbage collector

When a table reference is no longer referenced by any variable, it is of course eligible to be
garbage collected. If the table reference has not already been disposed in some other way, it will
be disposed at this time by the corresponding C# finalizer.

However, it is not a best practice to leave the disposal job with the garbage collector. This is
because the garbage collector is only concerned about memory pressure, not about any other kind of
resource usage. For example, a computer with a lot of memory may go a long time without running the
garbage collector. Also, for efficiency reasons, the garbage collector may decide to only collect
some garbage, leaving other garbage to clean up later. In short, the developer cannot know if or
when the collector will ever get around to cleaning up some specific C# object. Therefore, relying
on the garbage collector for resource management could cause table resources to needlessly
accumulate on the server side, which is wasteful and might even lead to resource exhaustion on the
server.

### Resource disposal with the .NET Dispose pattern

Table references implement the .NET `Dispose` pattern. When a program is finished with a table
reference, it can call `IQueryTable.Dispose`. Additionally, one can use the `using`
pattern to make this call convenient and automatic. For example in the above code, we could have
written

```c#
using (var table = scope.HistoricalTable("LearnDeephaven", "EODTrades"))
{
    ...
}

```

The main problem with this is approach is that we have to apply a `using` statement to both our
explicit and implicit table references. This can quickly become unwieldy:

```c#
using (var table = scope.HistoricalTable("LearnDeephaven", "EODTrades"))
{
    ...
    using (var temp1 = table
        .Where(importDate == "2017-11-01" && ticker.StartsWith("K")))
    using (var temp2 = temp1.Select(ticker, close, volume))
    using (var temp3 = temp2.Sort(volume.Descending()))
    using (var filtered = temp3.Head(10))
    {
        PrintUtils.PrintTableData(filtered);
    }
}
```

The formerly compact code is now littered with `using` statements and temporary variables.

### Resource disposal with the Open API IQueryScope pattern

Programs will almost always want to use the @Deephaven.OpenAPI.Client.IQueryScope pattern for resource disposal, as it is
easy, flexible, and convenient. The idea behind the @Deephaven.OpenAPI.Client.IQueryScope is that the job of table cleanup is
left to the @Deephaven.OpenAPI.Client.IQueryScope object, so the programmer need only worry about disposing the @Deephaven.OpenAPI.Client.IQueryScope
object rather than about every individual table reference. In this pattern, the program organizes
its tables into "resource groups" where the lifetime of each group is managed as a unit.

The simplest programs have a single @Deephaven.OpenAPI.Client.IQueryScope, that was created by the @Deephaven.OpenAPI.Client.IWorkerSession when the
program created or attached to the worker on the server. In this case, when there is only a single
@Deephaven.OpenAPI.Client.IQueryScope, resource disposal is straightforward: when the @Deephaven.OpenAPI.Client.IWorkerSession is disposed, it
disposes the @Deephaven.OpenAPI.Client.IQueryScope, which in turn disposes the <xref:Deephaven.OpenAPI.Client.IQueryTable>`s within its purview,
which in turn cleans up the corresponding server resources.

However, real-world programs will tend to have multiple <xref:Deephaven.OpenAPI.Client.IQueryScope>s. This allows the code to
control different lifetimes for different sets of tables. For example, the program may want some
tables to stay live for the duration of a program, others for the duration of a method, others
for the lifetime of a specific thread, and so on.

The ownership model is *shared ownership*. This means that at disposal time, an @Deephaven.OpenAPI.Client.IQueryScope will
dispose all the table references under its management *that are not being managed by any other
<xref:Deephaven.OpenAPI.Client.IQueryScope>*. This provides the programmer with a lot of flexibility, as the caller does not need
to decide the disposal order of table references *a priori*. Rather, they are disposed when their last owner
is disposed.

The program controls scopes through the way it uses @Deephaven.OpenAPI.Client.IQueryTable objects. Conceptually, an
@Deephaven.OpenAPI.Client.IQueryTable is an immutable pair (`TableHandle`, <xref:Deephaven.OpenAPI.Client.IQueryScope>) where a `TableHandle` refers to
server-side table resources, and @Deephaven.OpenAPI.Client.IQueryScope is the object that manages this
<xref:Deephaven.OpenAPI.Client.IQueryTable>.

@Deephaven.OpenAPI.Client.IQueryTable objects are created *de novo* by methods on @Deephaven.OpenAPI.Client.IQueryScope, or they are created as
derivations of other @Deephaven.OpenAPI.Client.IQueryTable objects. This approach allows for a great deal of flexibility:

* Methods like @Deephaven.OpenAPI.Client.IQueryScope.IntradayTable* and @Deephaven.OpenAPI.Client.IQueryScope.HistoricalTable*
  create a new @Deephaven.OpenAPI.Client.IQueryTable
  object bound to an existing @Deephaven.OpenAPI.Client.IQueryScope and a new server table resource.

* Methods like `qt`.<xref:Deephaven.OpenAPI.Client.IQueryTable.Where*>, `qt`.<xref:Deephaven.OpenAPI.Client.IQueryTable.Select*>,
  and `qt`.<xref:Deephaven.OpenAPI.Client.IQueryTable.Head*> create a new @Deephaven.OpenAPI.Client.IQueryTable object
  bound to the same @Deephaven.OpenAPI.Client.IQueryScope as `qt` but pointing to a new server table resource.
  This is the common and convenient way that server table
  resources end up being managed by the same @Deephaven.OpenAPI.Client.IQueryScope. Even a chained call like
  `qt.Select(...).Where(...).Head(...)`, in which there are anonymous compiler temporary variables,
  would end up with these variables managed by the same @Deephaven.OpenAPI.Client.IQueryScope.

* The method @Deephaven.OpenAPI.Client.IQueryScope.NewScope* creates a new scope. Despite outward appearances, this new scope is
  not a child or derived scope of the original in any way. Rather the original scope and new scope have a peer
  relationship; either might outlive the other. The reason @Deephaven.OpenAPI.Client.IQueryScope.NewScope* is defined as a method on
  @Deephaven.OpenAPI.Client.IQueryScope is because scopes need to know what @Deephaven.OpenAPI.Client.IWorkerSession (basically, what
  server worker) they are attached to; this is a convenient way to inherit that state without much effort by the programmer.

* The method `scope2`.<xref:Deephaven.OpenAPI.Client.IQueryScope.Manage*>`(t1)` creates a new
  @Deephaven.OpenAPI.Client.IQueryTable object with `t1`'s server `TableHandle`
  resource, but bound to the scope `scope2`. Put another way, this creates an object that refers to
  the same server table as `t1` but is managed (via the shared ownership model) by `scope2`.
  Recall that the "shared ownership" model means that the server resources will stay active
  until after both the original scope and `scope2` are disposed, and that those scopes may be disposed in
  either order.

* The convenience method <xref:Deephaven.OpenAPI.Client.IQueryTable.NewScope(Deephaven.OpenAPI.Client.IQueryTable@)>
  creates a fresh @Deephaven.OpenAPI.Client.IQueryScope object, and then
  binds that new scope and the original table's `TableHandle` object to it. It is equivalent to this code:
  ```c#
  var scope1 = qt1.Scope;
  var scope2 = scope1.NewScope();
  var qt2 = scope2.Manage(qt1);
  ```

  Note that `qt1` and `qt2` refer to the same server table resource, but belong to two different
  <xref:Deephaven.OpenAPI.Client.IQueryScope>s. The way this should be interpreted is that the
  server table can be accessed through `qt1` until such time as `scope1` is disposed, and
  the same server table can be accessed through `qt2` until such time as `scope2` is disposed.
  `scope1` and `scope2` can be disposed in either order.

  One big advantage of this entry point is that it can be used compactly in a `using` statement:
  ```c#
  using (qt1.NewScope(out var qt2))
  {
     ...
  }
  ```

The reader may be wondering why this is useful. Consider the following example:

```c#
using (var workerSession = client.StartWorker(workerOptions))
{
    var scope = workerSession.QueryScope;
    var table = scope.HistoricalTable("LearnDeephaven", "EODTrades");
    var (importDate, ticker) =
        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
    var nov1Data = table.Where(importDate == "2017-11-01");
    for (var ch = 'A'; ch <= 'Z'; ++ch)
    {
        using (nov1Data.NewScope(out var innerTable))
        {
            var chAsString = new string(ch, 1);
            Console.WriteLine($"Tickers starting with {chAsString}");
            var filtered = innerTable.Where(ticker.StartsWith(chAsString))
                .Head(10);
            PrintUtils.PrintTableData(filtered);
        }
    }
}
```

In this program, the `table` and `nov1Data` variables belong to the top-level scope owned by
`workerSession`, and therefore persist for basically the lifetime of the program (until
`workerSession` is disposed). Here we will call this the "outer scope". However, the `for` loop
repeatedly creates and destroys an inner scope (we will call this the "inner scope") for every
iteration.  The logic proceeds roughly like this:
* Create a new scope
* Make a new @Deephaven.OpenAPI.Client.IQueryTable object called `innerTable` that references `nov1Data's` `TableHandle` but
  belongs to this new scope
* Use `innerTable`.<xref:Deephaven.OpenAPI.Client.IQueryTable.Where*> to derive a new table from `innerTable`. This new table will also
  belong to the new scope
* Use .<xref:Deephaven.OpenAPI.Client.IQueryTable.Head*> to derive a new table from the above, also belonging to the new scope,
  and assigned to the variable `filtered`.
* Print the table called `filtered`.
* Upon leaving `newScope`'s `using` block, `newScope` will dispose the `TableHandles` that it
  *exclusively* owns, namely those `TableHandles` created by
  <xref:Deephaven.OpenAPI.Client.IQueryTable.Where*> and
  <xref:Deephaven.OpenAPI.Client.IQueryTable.Head*>.  But it will not dispose the `TableHandle`
  pointed to by `innerTable`, because that `TableHandle` is shared by `nov1Data` and is still being
  managed by the outer scope.

### Scopes are independent

In the examples above, our scopes happen to be nested due to the nature of our program.  However,
in reality <xref:Deephaven.OpenAPI.Client.IQueryScope>s are peers and can be disposed in whatever order the program sees fit.
Consider the following example:

```c#
using (var workerSession = client.StartWorker(workerOptions))
{
    var table = workerSession.QueryScope.HistoricalTable(
        "LearnDeephaven", "EODTrades");
    var (importDate, ticker) =
        table.GetColumns<StrCol, StrCol>("ImportDate", "Ticker");
    var t0 = table.Where(importDate == "2017-11-01" && ticker == "AAPL");
    var scope1 = filtered.NewScope(out var t1);
    var scope2 = t1.NewScope(out var t2);
    var scope3 = t2.NewScope(out var t3);
    // The variables t0, t1, t2, and t3 all refer to the same TableHandle at
    // the server.
    scope2.Dispose();
    // t2 and scope2 are invalid now, but the TableHandle is still live and can be
    // accessed via t0, t1, or t3.
    PrintUtils.PrintTableData(t3);
    scope3.Dispose();
    // t3 and scope3 are invalid now, but the TableHandle is still live and can be
    // accessed via t0 or t1.
    PrintUtils.PrintTableData(t1);
    scope1.Dispose();
    // t1 and scope1 are invalid now, but the TableHandle is still live and can be
    // accessed via t0.
}
```

## What's Next

In the next chapter, we cover the [Open API Fluent Interface](./fluent.md).
