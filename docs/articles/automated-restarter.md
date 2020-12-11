# Automated Restarter
> Creates an `InternalRoutine` adapted class to handle events and execute a process when threshold is achieved.

---

**`Automated Restarter`** can be used for procedures that check listeners with a countdown. Once an event listener matches with its timeout definition, then an action is invoked during process in parallel. Note that `Restarter` prefix on its name **doesn't** restart the application but could be used as one, see example below:

```csharp
private static readonly ManualResetEvent _mre = new ManualResetEvent(false);

public static void Main() {
  var restarter = new AutomatedRestarter(Timespan.FromSeconds(10), Timespan.FromSeconds(1).TotalMilliseconds, OnEventError);
  restarter.AddEventListeners(new[] {
    new KeyValuePair<TimeSpan, Action>(TimeSpan.FromSeconds(3), () => Announcement(3)),
    new KeyValuePair<TimeSpan, Action>(TimeSpan.FromSeconds(2), () => Announcement(2)),
    new KeyValuePair<TimeSpan, Action>(TimeSpan.FromSeconds(1), () => Announcement(1)),
  });
  restarter.OnFinished += RestartOperation;
  restarter.Start();
 
  _mre.WaitOne();
  
  Environment.Exit(0);
}

private static void OnEventError(string message) => Console.WriteLine($"An error occurred! -> message: {message}");

private static void Announcement(int seconds) => Console.WriteLine($"Preparing to restart within {seconds} second{(seconds > 1 ? "s" : "")}...");

private static void RestartOperation() {
  Thread.Sleep(1000);
  Process.Start(AppDomain.CurrentDomain.FriendlyName);
  
  _mre.Set();
}
```
