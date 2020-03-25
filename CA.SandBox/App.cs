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
                foreach (var entry in CommandList)
                    Info(string.Format(
                        "[{0}]. {1}{2}\n\t{3}\n",
                        entryId++,
                        entry.Key.Remove(0, 2),
                        $" (alias: {entry.Value.alias})" ?? "",
                        entry.Value.description
                    ));

                Tail();
                return;
            }

            Tail();
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