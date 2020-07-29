# [Async Procedure][ref-1]
> Used for situations that require dependency between other procedure. Recommended to use it with **`AsyncProcedurePool`**.

# [Async Procedure Pool][ref-2]
> Handle **`AsyncProcedure(TInput)`** instances into pool of synchronous or asynchronous routines.
---

**`Async Procedure`** can be used for procedures that require various operations (complex ones) and requires the output of all pending procedures to finish together. If a procedure breaks or stop working due an error then **`Async Procedure Pool`** will be set as invalid response flag type. See example below:

```csharp
public static void Main() {
  var procedures = new IAsyncProcedure[] {
    new AsyncProcedure<int>("proc::int", 1, OnProcedureExecute, OnProcedureError),
    new AsyncProcedure<float>("proc::float", 1.2f, OnProcedureExecute, OnProcedureError),
    new AsyncProcedure<double>("proc::double", 1.23d, OnProcedureExecute, OnProcedureError),
    new AsyncProcedure<DateTime>("proc::datetime", DateTime.UtcNow, OnProcedureExecute, OnProcedureError),
    new AsyncProcedure<object>("proc::object", new { unknown = 123 }, OnProcedureExecute, OnProcedureError)
  };
  var pool = new AsyncProcedurePool(procedures);
  var numProc = pool.NumProcedures;
  
  Console.WriteLine($"Executing {numProc} procedure{(numProc > 1 ? "s" : "")}...");
 
  var result = pool.ExecuteAllAsParallel();
  for (var i = 0; i < result.Length; i++)
    Console.WriteLine($"'{pool[i].GetName}' procedure -> result: {result[i]}");
  
  Console.ReadKey();
  Environment.Exit(0);
}

private static AsyncProcedureEventArgs<TInput> OnProcedureExecute(AsyncProcedure<TInput> procedure, string name, TInput input) {
  var timeout = 0;
  if (typeof(TInput) == typeof(int))
    timeout = 50;
  else if (typeof(TInput) == typeof(float))
    timeout = 100;
  else if (typeof(TInput) == typeof(double))
    timeout = 200;
  else if (typeof(TInput) == typeof(DateTime))
    timeout = 300;
  else {
    OnProcedureError("Not supported type! -> input: {input}, type: {typeof(input)}");
    return new AsyncProcedureEventArgs<TInput>(false, input);
  }
  
  Console.WriteLine($"Executing procedure '{name}'! -> input: {input}, type: {typeof(input)}, timeout: {timeout}ms");
  Thread.Sleep(timeout);
  return new AsyncProcedureEventArgs<TInput>(true, input);
}

private static void OnProcedureError(string message) => Console.WriteLine($"An error occurred! -> message: {message}");
```

[ref-1]: /CA/Threading/Tasks/Procedures/AsyncProcedure.cs
[ref-2]: /CA/Threading/Tasks/Procedures/AsyncProcedurePool.cs
