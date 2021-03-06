using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PSTk.Extensions.Utils
{
    /// <summary>
    /// Contains <see cref="string"/> utilities.
    /// </summary>
    public static class StringExtensions
    {
        private const string HumanReadablePattern = @"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])";
        private const string Vowels = "aeiou";

        /// <summary>
        /// Split <paramref name="text"/> within pieces of <paramref name="chunkSize"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<string> ChunkSplit(this string text, int chunkSize)
        {
            for (var i = 0; i < text.Length; i += chunkSize)
                yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
        }

        /// <summary>
        /// Verify if <see cref="char"/> contains in <see cref="Vowels"/>.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsVowel(this char c) => Vowels.Contains(c.ToString().ToLower());

        /// <summary>
        /// Convert <paramref name="text"/> to human readable pattern.
        /// </summary>
        /// <example>
        /// From: abc def ghi
        /// To: Abc Def Ghi
        /// </example>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToHumanReadable(this string text)
        {
            var upperCaseRgx = new Regex(HumanReadablePattern, RegexOptions.IgnorePatternWhitespace);
            return upperCaseRgx.Replace(text, " ");
        }

        /// <summary>
        /// Convert <paramref name="text"/> first character to upper case.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToUpperFirst(this string text)
            => char.ToUpper(text[0]) +
#if NET472
                text.Substring(1);
#else
                text[1..];

#endif
    }
}
