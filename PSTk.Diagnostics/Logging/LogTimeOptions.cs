using System;

namespace PSTk.Diagnostics.Logging
{
    /// <summary>
    /// Represents the <see cref="LogSlim{T}"/> options for time format pattern.
    /// </summary>
    [Flags]
    public enum LogTimeOptions
    {
        /// <summary>
        /// Apply default time format pattern to display year, month, day, hours, minutes and seconds.
        /// <para>
        /// Pattern: MM/dd/yyyy HH:mm:ss
        /// </para>
        /// </summary>
        None,

        /// <summary>
        /// Apply time format pattern to display hours, minutes and seconds.
        /// <para>
        /// Pattern: HH:mm:ss tt
        /// </para>
        /// </summary>
        Regular,

        /// <summary>
        /// Apply time format pattern to display year, month, day, hours, minutes and seconds.
        /// <para>
        /// Pattern: dddd, dd MMMM yyyy - HH:mm:ss tt
        /// </para>
        /// </summary>
        FullRegular,

        /// <summary>
        /// Apply time format pattern to display hours, minutes, seconds and milliseconds (four most significant digits of the seconds).
        /// <para>
        /// Pattern: HH:mm:ss:ffff
        /// </para>
        /// </summary>
        Scientific,

        /// <summary>
        /// Apply time format pattern to display year, month, day, hours, minutes, seconds and milliseconds (four most significant digits of the seconds).
        /// <para>
        /// Pattern: dddd, dd MMMM yyyy - HH:mm:ss:ffff
        /// </para>
        /// </summary>
        FullScientific,

        /// <summary>
        /// Apply time format pattern to display year, month, day, hours and minutes.
        /// <para>
        /// Pattern: MM/dd/yyyy hh:mm tt
        /// </para>
        /// </summary>
        Classic,

        /// <summary>
        /// Apply time format pattern to display year, month, day, hours, minutes and seconds.
        /// <para>
        /// Pattern: MM/dd/yyyy hh:mm:ss tt
        /// </para>
        /// </summary>
        ClassicRegular,

        /// <summary>
        /// Apply time format pattern to display year, month, day, hours, minutes, seconds and milliseconds (four most significant digits of the seconds).
        /// <para>
        /// Pattern: MM/dd/yyyy hh:mm:ss:ffff tt
        /// </para>
        /// </summary>
        ClassicScientific
    }
}
