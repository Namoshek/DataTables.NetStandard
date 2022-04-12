using System;
using System.Linq;

namespace DataTables.NetStandard.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// If the given <paramref name="input"/> is not <c>null</c> and not empty, returns a string with the first character in upper case.
        /// For <c>null</c> and empty strings, an exception is thrown.
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentNullException">If <paramref name="input"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="input"/> is empty.</exception>
        public static string FirstCharToUpper(this string input)
        {
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                
                _ => input.First().ToString().ToUpper() + input[1..],
            };
        }
    }
}
