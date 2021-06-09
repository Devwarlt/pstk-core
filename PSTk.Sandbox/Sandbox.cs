using PSTk.Diagnostics.Logging;
using PSTk.Extensions.Utils;
using PSTk.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PSTk.Sandbox
{
    public sealed class Sandbox
    {
        private static HandlerRoutine _ctrlHandler;
        private static CancellationTokenSource _cts;
        private static LogSlim<Sandbox> _logger;

        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        [Flags]
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public static DateTime TotalElapsedTime => Process.GetCurrentProcess().StartTime;

        private static string _clrRuntimeVersion => Assembly.GetExecutingAssembly().ImageRuntimeVersion;

        private static string _domainName => AppDomain.CurrentDomain.FriendlyName;

        private static void Main()
        {
            _cts = new CancellationTokenSource();
            _logger = new LogSlim<Sandbox>(LogTimeOptions.ClassicScientific, true, true);
            _logger.OnTerminate += (s, e) => Terminate(string.IsNullOrWhiteSpace(e.Message) ? 0 : 1);
            _ctrlHandler += (ctrlType) =>
            {
                if (ctrlType is CtrlTypes.CTRL_C_EVENT
                    or CtrlTypes.CTRL_BREAK_EVENT
                    or CtrlTypes.CTRL_CLOSE_EVENT
                    or CtrlTypes.CTRL_LOGOFF_EVENT
                    or CtrlTypes.CTRL_SHUTDOWN_EVENT)
                {
                    Terminate();
                    return false;
                }
                return true;
            };

            SetConsoleCtrlHandler(_ctrlHandler, true);

            _logger.Print("Running an internal routine event...");

            var internalRoutine = new InternalRoutine($"{nameof(Sandbox)}::internalRoutine()", 200, OnTickDelta, _logger.PrintErr);
            internalRoutine.OnDeltaVariation += (s, e) => e.TrackDelta(_logger);
            internalRoutine.AttachToParent(_cts.Token);
            internalRoutine.Start();

            var mre = new ManualResetEventSlim(false);
            mre.Wait();
        }

        private static void OnTickDelta(long delta) => _logger.PrintDbg($"Delta -> {delta}");

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        private static void Terminate(int exitCode = 0)
        {
            _logger.Print("Terminating...");
            _logger.PrintWarn($"{TotalElapsedTime.GetElapsedTime($"'{_domainName}' ran for ")}");

            var finalizers = new[]
            {
                Task.Run(() => _cts.Cancel())
            };
            Task.WaitAll(finalizers, TimeSpan.FromSeconds(30));

            _logger.Print("Terminated!");
            Environment.Exit(exitCode);
        }
    }
}
