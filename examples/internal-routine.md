# [Internal Routine][ref-1]
> Used for synchronous or asynchronous routine.
---

**`Internal Routine`** can be used for cyclic operations running in parallel as loop process, see example below:

```csharp
private const int _threshold = 10;

private static int i = 0;

private static readonly ManualResetEvent _mre = new ManualResetEvent(false);
private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

public static void Main() {
  // note that internal routine uses timeout in milliseconds
  var routine = new InternalRoutine(Timespan.FromSeconds(1).TotalMilliseconds, OnTickDelta, OnTickError);
  routine.AttachToParent(_cts.Token);
  routine.OnDeltaVariation += OnTickDelay;
  routine.OnFinished += OnTickOver;
  routine.Start();
  
  _mre.WaitOne();
  
  Environment.Exit(0);
}

private static void OnTickDelta(int delta) {
  if (!_cts.IsCancellationRequested && ++i == _threshold) {
    _cts.Cancel();
    return;
  }
}

private static void OnTickError(string message) => Console.WriteLine($"An error occurred! -> message: {message}");

private static void OnTickDelay(object sender, InternalRoutineEventArgs args)
  => Console.WriteLine(
      "Long tick detected! -> " +
      $"delay: {args.Delta}ms, " +
      $"timeout: {args.Timeout}ms, " +
      $"ticks per second: {args.TicksPerSecond}"
  );

private static void OnFinished(object sender, EventArgs args) {
  Console.WriteLine("Finished routine!");
  _mre.Set();
}
```

[ref-1]: /CA/Threading/Tasks/InternalRoutine.cs
