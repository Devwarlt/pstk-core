using System;

namespace PSTk.Diagnostics.Logging
{
    /// <summary>
    /// Use <see cref="Console"/> features to display messages. This is a lightweight version of loggers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class LogSlim<T> : ILog<T>
    {
        /// <summary>
        /// This delegate is invoked when <see cref="OnUnhandledException(object, UnhandledExceptionEventArgs)"/> is triggered.
        /// Use this event to run further application termination callbacks.
        /// </summary>
        public EventHandler<TerminateEventArgs> OnTerminate;

        private const string Dbg = "DEBUG";
        private const string Err = "ERROR";
        private const string Ftl = "FATAL";
        private const string Info = "INFO";
        private const string Wrn = "WARN";

        private readonly bool terminateOnError;
        private readonly string timePattern;

        /// <summary>
        /// Create a new instance of <see cref="LogSlim{T}"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="terminateOnError"></param>
        public LogSlim(LogTimeOptions options, bool terminateOnError = false)
            : this(options, terminateOnError, false)
        { }

        /// <summary>
        /// Create a new instance of <see cref="LogSlim{T}"/> with extra features.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="terminateOnError"></param>
        /// <param name="configureUnhandledExceptions"></param>
        public LogSlim(LogTimeOptions options, bool terminateOnError, bool configureUnhandledExceptions = false)
        {
            this.terminateOnError = terminateOnError;
            timePattern = GenerateTimePattern(options);
            if (configureUnhandledExceptions)
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// Breakline invoking <see cref="Console.WriteLine()"/>.
        /// </summary>
        public void Endl() => OnConsoleLog(string.Empty, string.Empty);

        /// <summary>
        /// Catch unhandled exceptions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
            => PrintUnhandled(sender, args.ExceptionObject as Exception);

        /// <summary>
        /// Display <paramref name="message"/> on <see cref="Console"/> with level <see cref="Info"/>.
        /// </summary>
        /// <param name="message"></param>
        public void Print(string message) => OnConsoleLog(Info, message);

        /// <summary>
        /// Display <paramref name="message"/> on <see cref="Console"/> with level <see cref="Dbg"/>.
        /// </summary>
        /// <param name="message"></param>
        public void PrintDbg(string message) => OnConsoleLog(Dbg, message, ConsoleColor.DarkGray);

        /// <summary>
        /// Display <paramref name="message"/> on <see cref="Console"/> with level <see cref="Err"/>.
        /// </summary>
        /// <param name="message"></param>
        public void PrintErr(string message) => OnConsoleLog(Err, message, ConsoleColor.Red);

        /// <summary>
        /// Display <paramref name="message"/> and <paramref name="exception"/> on <see cref="Console"/> with level <see cref="Err"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void PrintErr(string message, Exception exception) => OnConsoleLog(Err, message, ConsoleColor.Red, exception);

        /// <summary>
        /// Display <paramref name="message"/> and <paramref name="exception"/> on <see cref="Console"/> with level <see cref="Ftl"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void PrintFtl(string message, Exception exception) => OnConsoleLog(Ftl, message, ConsoleColor.DarkRed, exception);

        /// <summary>
        /// Display <paramref name="message"/> on <see cref="Console"/> with level <see cref="Wrn"/>.
        /// </summary>
        /// <param name="message"></param>
        public void PrintWarn(string message) => OnConsoleLog(Wrn, message, ConsoleColor.Yellow);

        private static string GenerateTimePattern(LogTimeOptions options)
#if NET472
        {
            switch (options)
            {
                case LogTimeOptions.Classic: return "MM/dd/yyyy hh:mm tt";
                case LogTimeOptions.ClassicRegular: return "MM/dd/yyyy hh:mm:ss tt";
                case LogTimeOptions.ClassicScientific: return "MM/dd/yyyy hh:mm:ss:ffff tt";
                case LogTimeOptions.FullRegular: return "dddd, dd MMMM yyyy - HH:mm:ss tt";
                case LogTimeOptions.FullScientific: return "dddd, dd MMMM yyyy - HH:mm:ss:ffff";
                case LogTimeOptions.Regular: return "HH:mm:ss tt";
                case LogTimeOptions.Scientific: return "HH:mm:ss:ffff";
                default: return string.Empty;
            }
        }
#else
            => options switch
            {
                LogTimeOptions.Classic => "MM/dd/yyyy hh:mm tt",
                LogTimeOptions.ClassicRegular => "MM/dd/yyyy hh:mm:ss tt",
                LogTimeOptions.ClassicScientific => "MM/dd/yyyy hh:mm:ss:ffff tt",
                LogTimeOptions.FullRegular => "dddd, dd MMMM yyyy - HH:mm:ss tt",
                LogTimeOptions.FullScientific => "dddd, dd MMMM yyyy - HH:mm:ss:ffff",
                LogTimeOptions.Regular => "HH:mm:ss tt",
                LogTimeOptions.Scientific => "HH:mm:ss:ffff",
                _ => string.Empty
            };

#endif

        private string LogHeader(string level) => $"[{LogTimer()}] | [{level}] | <{typeof(T).Name}> ";

        private string LogTimer() => DateTime.Now.ToString(timePattern);

        private void OnConsoleLog(string level, string message, ConsoleColor color = ConsoleColor.White, Exception exception = null)
        {
            if (string.IsNullOrWhiteSpace(level))
            {
                Console.WriteLine();
                return;
            }

            Console.ForegroundColor = color;
            Console.WriteLine($"{LogHeader(level)}{message}");

            if (exception != null)
                Console.WriteLine(exception);

            Console.ResetColor();
        }

        private void PrintUnhandled(object sender, Exception exception)
        {
            PrintFtl(
                $"{(sender != null ? $"[{sender}] " : string.Empty)}Unhandled Exception" +
                $" (terminate: {terminateOnError})!",
                exception
            );

            if (terminateOnError)
                OnTerminate?.Invoke(sender, new TerminateEventArgs("An unhandled exception stopped application execution."));
        }
    }
}
