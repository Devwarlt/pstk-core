using CA.Threading.Tasks;
using CA.Threading.Tasks.Procedures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CA.SandBox
{
    public class App
    {
        private static readonly Dictionary<string, (string alias, string description, Action<string> action)> CommandList
            = new Dictionary<string, (string alias, string description, Action<string> action)>()
            {
                { "--help", ("h", "Show a list of all commands.", (input) => HandleHelp()) },
                { "--test", ("t", "Execute a specific test.", (input) => HandleTest(input)) }
            };

        private static readonly Dictionary<string, (string description, Action<string[]> action)> TestCommandList
            = new Dictionary<string, (string description, Action<string[]> action)>()
            {
                {
                    "-c1",
                    (
                        "Execute a test for class 'InternalRoutine', for options:" +
                        "\n\t[1].\ttimeout: 1000ms, total elapsed time: 100s" +
                        "\n\t[2].\ttimeout: 500ms, total elapsed time: 50s" +
                        "\n\t[3].\ttimeout: 250ms, total elapsed time: 25s" +
                        "\n\t[4].\ttimeout: 125ms, total elapsed time: 12.5s",
                        (args) => HandleTestC1Options(args, numRequiredArgs: 1)
                    )
                },
                {
                    "-c2",
                    (
                        "Execute a test for class 'AsyncProcedure' and 'AsyncProcedurePool', for options:" +
                        "\n\t[1].\t2 async procedures in parallel" +
                        "\n\t[2].\t4 async procedures in parallel" +
                        "\n\t[3].\t8 async procedures in parallel" +
                        "\n\t[4].\t16 async procedures in parallel" +
                        "\n\t[5].\t32 async procedures in parallel" +
                        "\n\t[6].\t64 async procedures in parallel" +
                        "\n\t[7].\t128 async procedures in parallel" +
                        "\n\t[8].\t256 async procedures in parallel",
                        (args) => HandleTestC2Options(args, numRequiredArgs: 1)
                    )
                },
                {
                    "-c3",
                    (
                        "Execute a test for class 'AutomatedRestarter', for options:" +
                        "\n\t[1].\t30 seconds" +
                        "\n\t[2].\t1 minute" +
                        "\n\t[3].\t2 minutes" +
                        "\n\t[4].\t3 minutes" +
                        "\n\t[5].\t4 minutes" +
                        "\n\t[6].\t5 minutes",
                        (args) => HandleTestC3Options(args, numRequiredArgs: 1)
                    )
                }
            };

        private static Dictionary<string, string> Commands = new Dictionary<string, string>();
        private static Dictionary<string, string> Aliases = new Dictionary<string, string>();

        private static void Main(string[] args)
        {
            var name = Assembly.GetExecutingAssembly().GetName().Name;
            var version =
                $"{Assembly.GetExecutingAssembly().GetName().Version}".Substring(0,
                $"{Assembly.GetExecutingAssembly().GetName().Version}".Length - 2);

            Console.Title = $"{name} v{version} - The sandbox environment for Core Algorithms DLL tests.";

            var mre = new ManualResetEvent(false);

            Console.CancelKeyPress += delegate { mre.Set(); };

            Warn("Initializing...");
            Breakline();

            Task.Factory.StartNew(() => Core(name), TaskCreationOptions.AttachedToParent);

            mre.WaitOne();

            Warn("Terminating...");

            Thread.Sleep(500);
            Environment.Exit(0);
        }

        #region "Core"

        private static void Core(string name)
        {
            Info("Loading commands and aliases...");

            foreach (var entry in CommandList)
            {
                var command = entry.Key;
                var alias = $"--{entry.Value.alias}";

                Commands.Add(command, alias);
                Aliases.Add(alias, command);
            }

            Info($"Loaded {CommandList.Count} command{(CommandList.Count > 1 ? "s" : "")}.");
            Breakline();
            Info($"{name} is ready to use!");
            Breakline();
            Warn("Press ANY key to continue...");

            Console.ReadKey(true);

            CoreLoop();
        }

        private static void CoreLoop()
        {
            Console.Clear();

            DisplayTitle();
            Breakline();
            Breakline();
            Info("Type --help for more details.");
            Breakline();

            var input = Console.ReadLine();
            var args = input.Split(' ');

            if (args.Length == 0)
            {
                Breakline();

                Console.Clear();

                CoreLoop();
                return;
            }

            var command = args[0];
            var newInput = string.Join(" ", args.Skip(1));

            Console.Clear();

            DisplayTitle();
            Breakline();
            Breakline();

            if (Commands.ContainsKey(command)) CommandList[command].action(newInput);
            else if (Aliases.ContainsKey(command)) CommandList[Aliases[command]].action(newInput);
            else CoreLoop();
        }

        #endregion "Core"

        #region "Command Handlers"

        private static void HandleHelp()
        {
            Info(
                "CA SandBox is an environment for Core Algorithms stress tests. " +
                "Use it wisely and try to avoid application crashing issues."
            );
            Breakline();

            var entryId = 1;

            foreach (var entry in CommandList)
                Info(string.Format(
                    "[{0}]. {1}{2}\n\t{3}\n",
                    entryId++,
                    entry.Key.Remove(0, 2),
                    $" (alias: {entry.Value.alias})" ?? "",
                    entry.Value.description
                ));

            Tail();
        }

        private static void HandleTest(string input)
        {
            var entryId = 1;

            if (string.IsNullOrWhiteSpace(input))
            {
                foreach (var entry in TestCommandList)
                    Info(string.Format(
                        "[{0}]. {1}\n\t\t{2}\n\n{3}\n",
                        entryId++,
                        entry.Key,
                        entry.Value.description,
                        $"\tUsage: --test {entry.Key} [option] (without brackets)"
                    ));

                Tail();
                return;
            }

            var args = input.Split(' ');

            if (args.Length == 0)
            {
                Tail();
                return;
            }

            var command = args[0];

            if (!TestCommandList.ContainsKey(command))
            {
                Error("Invalid option!");
                Tail();
                return;
            }

            var newInput = args.Skip(1).ToArray();

            TestCommandList[command].action.Invoke(newInput);
        }

        private static void HandleTestC1Options(string[] args, int numRequiredArgs)
        {
            if (args.Length < numRequiredArgs)
            {
                Error($"This command requires {numRequiredArgs} extra argument{(numRequiredArgs > 1 ? "s" : "")}.");
                Tail();
                return;
            }

            var option = args[0];
            var timeout = -1;

            switch (option)
            {
                case "1": timeout = 1000; break;
                case "2": timeout = 500; break;
                case "3": timeout = 250; break;
                case "4": timeout = 125; break;
                default:
                    Error($"Invalid option: {option}");
                    Tail();
                    return;
            }

            var (min, max) = (1f, 100f);
            var i = min;
            var displayed100Percent = false;
            var isCompleted = false;
            var isForced = false;
            void progress()
            {
                displayed100Percent = i == max;

                Info($"[progressRoutine] {i}/{max} {(i / max).ToString("##.00%")}");
            }

            var source = new CancellationTokenSource();
            var syncForced = new ManualResetEvent(false);
            var progressRoutine = new InternalRoutine(1000, progress);
            progressRoutine.AttachToParent(source.Token);
            progressRoutine.OnInitializing += (s, e) => Info("[progressRoutine] Initializing progress routine...");
            progressRoutine.OnFinished += (s, e) =>
            {
                Info("[progressRoutine] Stopping progress routine...");

                syncForced.Set();
            };

            var incrementRoutine = new InternalRoutine(timeout, (routine) =>
            {
                if (i < max) ++i;

                if (i == max) source.Cancel();
            });
            incrementRoutine.AttachToParent(source.Token);
            incrementRoutine.OnInitializing += (s, e) =>
            {
                Info("[incrementRoutine] Initializing increment routine...");
                Info(
                    $"[incrementRoutine] Starting incrementing from {min} to {max} " +
                    $"every {timeout} ms (ETA to finish this task: " +
                    $"{(max * timeout / 1000f).ToString("##.00")}s)."
                );

                progressRoutine.Start();
            };
            incrementRoutine.OnFinished += (s, e) =>
            {
                Info("[incrementRoutine] Stopping increment routine...");

                if (isForced) return;

                if (!displayed100Percent) progress();

                isCompleted = true;

                Breakline();
                Warn("[Success] All routines have been stopped: 'incrementRoutine' and 'progressRoutine'");
                Tail();
            };

            Warn("All routines have been started: 'incrementRoutine' and 'progressRoutine'");
            Breakline();
            Warn("Press ANY key to stop all internal routines...");
            Breakline();

            incrementRoutine.Start();

            Console.ReadKey(true);

            if (!isCompleted)
            {
                isForced = true;

                source.Cancel();
                syncForced.WaitOne();

                Breakline();
                Warn("[Forced] All routines have been stopped: 'incrementRoutine' and 'progressRoutine'");
                Tail();
            }
        }

        private static void HandleTestC2Options(string[] args, int numRequiredArgs)
        {
            if (args.Length < numRequiredArgs)
            {
                Error($"This command requires {numRequiredArgs} extra argument{(numRequiredArgs > 1 ? "s" : "")}.");
                Tail();
                return;
            }

            var option = args[0];
            var numProcedures = -1;

            switch (option)
            {
                case "1": numProcedures = 2; break;
                case "2": numProcedures = 4; break;
                case "3": numProcedures = 8; break;
                case "4": numProcedures = 16; break;
                case "5": numProcedures = 32; break;
                case "6": numProcedures = 64; break;
                case "7": numProcedures = 128; break;
                case "8": numProcedures = 256; break;
                default:
                    Error($"Invalid option: {option}");
                    Tail();
                    return;
            }

            var rand = new Random();
            var (maxMin, maxMax) = (25, 100);
            var pool = new IAsyncProcedure[numProcedures];
            var intRange = Enumerable.Range(rand.Next(0, rand.Next(maxMin, maxMax)), numProcedures).ToArray();
            var source = new CancellationTokenSource();

            for (var i = 0; i < numProcedures; i++)
            {
                var procedure = new AsyncProcedure<int>(
                    $"AsyncProcedure#{i + 1}",
                    intRange[i],
                    (instance, name, input) =>
                    {
                        var timeout = rand.Next(50, 200);
                        var toIncrement = 100;

                        Info(
                               $"[{name}] Starting procedure: increment {input} in {(toIncrement > 0 ? "+" : "-")}" +
                               $"{toIncrement} unit{(Math.Abs(toIncrement) > 1 ? "s" : "")} (timeout: " +
                               $"{timeout}, ETA: {(toIncrement * timeout / 1000f).ToString("##.00")}s)."
                           );

                        var initialized = new ManualResetEvent(false);
                        var finished = new ManualResetEvent(false);
                        var j = 0;
                        var procedureSource = new CancellationTokenSource();
                        var newInput = input;
                        var routine = new InternalRoutine(timeout, (thisRoutine) =>
                        {
                            if (j++ == toIncrement)
                            {
                                procedureSource.Cancel();
                                Warn($"[{name}] Stopping internal routine.");
                                return;
                            }

                            newInput++;
                        });
                        routine.AttachToParent(procedureSource.Token);
                        routine.OnInitialized += (s, e) => initialized.Set();
                        routine.OnFinished += (s, e) => finished.Set();
                        routine.Start();

                        initialized.WaitOne();
                        finished.WaitOne();

                        return new AsyncProcedureEventArgs<int>(newInput, true);
                    }
                );
                procedure.AttachToParent(source.Token);
                procedure.OnCompleted += (s, e) => Warn($"[{((IAsyncProcedure)s).GetName}] Finished procedure with result '{e.Input}'.");
                pool[i] = procedure;
            }

            Breakline();
            Warn("Press ANY key to stop all procedures...");
            Breakline();

            var procedurePool = new AsyncProcedurePool(pool, source);
            var isCompleted = false;

            Task.Run(() =>
            {
                Breakline();
                Warn(
                    $"All procedures have been started: {procedurePool.NumProcedures} " +
                    $"async procedure{(procedurePool.NumProcedures > 1 ? "s" : "")}"
                );
                Breakline();

                var results = procedurePool.ExecuteAllAsParallel();

                isCompleted = true;

                Breakline();
                Warn(
                    $"[Success] All procedures have been stopped: {procedurePool.NumProcedures} " +
                    $"async procedure{(procedurePool.NumProcedures > 1 ? "s" : "")}"
                );
                Breakline();
                Info("Displaying results:");

                for (var i = 0; i < procedurePool.NumProcedures; i++)
                    Info($"[name: {procedurePool[i].GetName}] Result: {(results[i] ? "success" : "failed")}");

                Tail();
            });

            Console.ReadKey(true);

            if (!isCompleted)
            {
                procedurePool.CancelAll();

                Breakline();
                Warn(
                    $"[Forced] All procedures have been stopped: {procedurePool.NumProcedures} " +
                    $"async procedure{(procedurePool.NumProcedures > 1 ? "s" : "")}"
                );
                Tail();
            }
        }

        private static void HandleTestC3Options(string[] args, int numRequiredArgs)
        {
            if (args.Length < numRequiredArgs)
            {
                Error($"This command requires {numRequiredArgs} extra argument{(numRequiredArgs > 1 ? "s" : "")}.");
                Tail();
                return;
            }

            var option = args[0];
            TimeSpan timeout;

            switch (option)
            {
                case "1": timeout = TimeSpan.FromSeconds(30); break;
                case "2": timeout = TimeSpan.FromMinutes(1); break;
                case "3": timeout = TimeSpan.FromMinutes(2); break;
                case "4": timeout = TimeSpan.FromMinutes(3); break;
                case "5": timeout = TimeSpan.FromMinutes(4); break;
                case "6": timeout = TimeSpan.FromMinutes(5); break;
                default:
                    Error($"Invalid option: {option}");
                    Tail();
                    return;
            }

            var isCompleted = false;
            var totalMs = timeout.TotalMilliseconds;
            var time75p = TimeSpan.FromMilliseconds(totalMs * 0.75);
            var time50p = TimeSpan.FromMilliseconds(totalMs * 0.5);
            var time25p = TimeSpan.FromMilliseconds(totalMs * 0.25);
            void log(TimeSpan time) => Info($"[AutomatedRestarter] Messaged notify when remains {time.TotalMilliseconds} ms to stop routine.");
            var restarter = new AutomatedRestarter(timeout);
            restarter.AddEventListeners(new[]
            {
                new KeyValuePair<TimeSpan, Action>(timeout, () => log(timeout)),
                new KeyValuePair<TimeSpan, Action>(time75p, () => log(time75p)),
                new KeyValuePair<TimeSpan, Action>(time50p, () => log(time50p)),
                new KeyValuePair<TimeSpan, Action>(time25p, () => log(time25p))
            });
            restarter.OnFinished += (s, e) =>
            {
                isCompleted = true;

                Breakline();
                Warn("[Success] Automated restarter have been stopped.");
                Tail();
            };

            Info($"[AutomatedRestarter] This process will stop within {totalMs} ms...");
            Breakline();
            Warn("Press ANY key to stop automated restarter...");
            Breakline();

            restarter.Start();

            Console.ReadKey(true);

            if (!isCompleted)
            {
                restarter.Stop();

                Breakline();
                Warn("[Forced] Automated restarter have been stopped.");
                Tail();
            }
        }

        #endregion "Command Handlers"

        private static void DisplayTitle()
            => Warn(
@"
                   .d8888b.         d8888
                  d88P  Y88b       d88888
                  888    888      d88P888
                  888            d88P 888
                  888           d88P  888
                  888    888   d88P   888
                  Y88b  d88P  d8888888888
                    Y8888P  d88888  88888
                                888 888
                                888 888
.d8888b   8888b.  88888b.   .d88888 88888b.   .d88b.  888  888
88K           88b 888  88b d88  888 888  88b d88  88b  Y8bd8P
Y8888b.  .d888888 888  888 888  888 888  888 888  888   X88K
     X88 888  888 888  888 Y88b 888 888 d88P Y88..88P .d8""8b.
.o8888P  Y888888  888  888   Y88888 88888P     Y88P   888  888
"
            );

        #region "Log utilities"

        private static void Tail(bool goToMenu = true)
        {
            Breakline();

            if (goToMenu)
            {
                Warn("Press ANY key to continue...");

                Console.ReadKey(true);

                CoreLoop();
            }
        }

        private static void Breakline() => Console.WriteLine("\n");

        private static void Info(string message, bool isWriteLine = true) => Log(message, isWriteLine, ConsoleColor.Gray);

        private static void Warn(string message, bool isWriteLine = true) => Log(message, isWriteLine, ConsoleColor.DarkYellow);

        private static void Error(string message, bool isWriteLine = true) => Log(message, isWriteLine, ConsoleColor.DarkRed);

        private static void Log(string message, bool isWriteLine, ConsoleColor color)
        {
            Console.ForegroundColor = color;

            if (isWriteLine) Console.WriteLine(message);
            else Console.Write(message);

            Console.ResetColor();
        }

        #endregion "Log utilities"
    }
}
