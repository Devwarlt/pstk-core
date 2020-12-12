using System;
using PSTk.Diagnostics.Logging;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains <see cref="IDisposable"/> utilities.
    /// </summary>
    public static class IDisposableExtensions
    {
        /// <summary>
        /// Dispose a class that implements interface <see cref="IDisposable"/> and track message on <see cref="ILog{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="N"></typeparam>
        /// <param name="disposable"></param>
        /// <param name="log"></param>
        public static void DisposeAndTrack<T, N>(this T disposable, ILog<N> log) where T : IDisposable
        {
            var status = $"Disposing '{typeof(T).Name}'...";

            try
            {
                log.PrintWarn(status);
                disposable.Dispose();
            }
            catch (Exception e)
            {
                log.PrintErr($"{status} [error!]", e);
                return;
            }

            log.Print($"{status} [ok!]");
        }
    }
}
