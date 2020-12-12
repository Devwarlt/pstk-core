using System;

namespace PSTk.Diagnostics.Logging
{
#pragma warning disable

    public interface ILog<T>
    {
        void OnUnhandledException(object sender, UnhandledExceptionEventArgs args);

        void Print(string message);

        void Endl();

        void PrintDbg(string message);

        void PrintErr(string message);

        void PrintErr(string message, Exception exception);

        void PrintFtl(string message, Exception exception);

        void PrintWarn(string message);
    }

#pragma warning restore
}
