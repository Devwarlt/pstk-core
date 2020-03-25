using CA.Threading.Tasks;
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
                        (args) => HandleTestC1Options(args)
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

        /*
            "Execute a test for class 'InternalRoutine', for options:" +
            "\n\t[1].\ttimeout: 1000ms, total elapsed time: 100s" +
            "\n\t[2].\ttimeout: 500ms, total elapsed time: 50s" +
            "\n\t[3].\ttimeout: 250ms, total elapsed time: 25s" +
            "\n\t[4].\ttimeout: 125ms, total elapsed time: 12.5s",
        */

        private static void HandleTestC1Options(string[] args)
        {
            if (args.Length < 1)
            {
                Error("This command requires one extra argument.");
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
            void progress()
            {
                displayed100Percent = i == max;

                Info($"[onProgress] {i}/{max} {(i / max).ToString("##.00%")}");
            }
            var onProgressRoutine = new InternalRoutine(1000, (routine) => progress());
            var onTestingRoutine = new InternalRoutine(timeout, (routine) =>
            {
                if (i < max) i++;

                if (i == max) routine.Stop();
            });
            var whenCompleteRoutine = new InternalRoutine(200, (routine) =>
            {
                if (i < max) return;

                if (i == max && (onProgressRoutine.IsRunning || onTestingRoutine.IsRunning))
                {
                    onProgressRoutine.Stop();
                    onTestingRoutine.Stop();
                    return;
                }

                if (!displayed100Percent) progress();

                isCompleted = true;

                Warn("[whenComplete] The test has been completed its routine!");

                routine.Stop();

                Breakline();
                Warn("All routines have been stopped: 'onTesting', 'onProgress' and 'whenComplete'");
                Tail();
            });

            Info($"Starting tests for option: {option}");
            Info(
                $"Starting incrementing from {min} to {max} every {timeout} ms (ETA to finish this " +
                $"task: {(max * timeout / 1000f).ToString("##.00")}s)."
            );
            Breakline();
            Warn("Press ANY key to stop 'onTesting', 'onProgress' and 'whenComplete' routines...");
            Breakline();

            onProgressRoutine.Start();
            onTestingRoutine.Start();
            whenCompleteRoutine.Start();

            Console.ReadKey(true);

            if (!isCompleted)
            {
                onProgressRoutine.Stop();
                onTestingRoutine.Stop();
                whenCompleteRoutine.Stop();

                Breakline();
                Warn("All routines have been stopped: 'onTesting', 'onProgress' and 'whenComplete'");
                Breakline();
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
                    Y8888P  d88888 88888
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