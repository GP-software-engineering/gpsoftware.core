using System;
using System.Text.RegularExpressions;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref="string"/>
    /// </summary>
    public static class StringExtension {

        /// <summary>
        ///     Reverse a string (last character will be the first, second last will be the second, ...)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Reverse(this string source) {
            if (string.IsNullOrEmpty(source)) return source;
            char[] charArray = source.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        ///     Case insensitive contains
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        ///     Returns a substring of this string or an empty string, if startIndex is major than string length
        /// </summary>
        public static string SubstringOrEmpty(this string str, int startIndex) {
            return str.SubstringWithAppendix(startIndex, str.Length, string.Empty);
        }

        /// <summary>
        ///     Returns a substring of this string or an empty string if startIndex is major than string length.
        /// </summary>
        public static string SubstringOrEmpty(this string str, int startIndex, int length) {
            return str.SubstringWithAppendix(startIndex, length, string.Empty);
        }

        /// <summary>
        ///     Returns a substring of this string ending with the passed <paramref name="appendix"/>
        ///     or just the <paramref name="appendix"/>> if startIndex is major than string length.
        /// </summary>
        public static string SubstringWithAppendix(this string str, int startIndex, int length, string appendix) {
            if (string.IsNullOrEmpty(str)) return string.Empty;
            if (startIndex >= str.Length) return appendix ?? string.Empty;

            var orgStrLen = str.Length;
            str = str.Substring(startIndex);

            if (length >= (str.Length)) return str;

            if ((length + appendix.Length) > str.Length) length = orgStrLen - appendix.Length;

            return str.Substring(0, length) + appendix;
        }

        /// <summary>
        ///     Returns a substring up to the FIRST occurrence of the passed <paramref name="value"/>. If <paramref name="value"/>
        ///     is not found, returns the whole string.
        /// </summary>
        public static string SubstringIndexOf(this string str, string value, int startIndex = 0) {
            var idx = str.IndexOf(value, startIndex);
            return (idx < 0) ? str : str.Substring(startIndex, idx);
        }

        /// <summary>
        ///     Returns a substring up to the LAST occurrence of the passed <paramref name="value"/>. If <paramref name="value"/>
        ///     is not found, returns the whole string.
        /// </summary>
        public static string SubstringLastIndexOf(this string str, char value, int startIndex = 0) {
            var idx = str.LastIndexOf(value);
            return (idx < 0) ? str : str.Substring(startIndex, idx);
        }

        /// <summary>
        ///     Returns a number that represents how many occurrences of the passed value are in a string
        /// </summary>
        public static int Count(this string str, string value, int startIndex = 0) {
            var source = (startIndex > 0) ? str.Substring(startIndex) : str;
            return (source.Length - source.Replace(value, string.Empty).Length);
        }

        /// <summary>
        ///     Returns a number that represents how many occurrences of the passed value are in a string
        /// </summary>
        public static int Count(this string str, char value, int startIndex = 0) {
            var source = (startIndex > 0) ? str.Substring(startIndex).ToCharArray() : str.ToCharArray();
            var sourceLen = source.Length;
            int count = 0;
            for (int i = 0; i < sourceLen; i++) if (source[i] == value) count++;
            return count;
        }

        /// <summary>
        ///     Adds a char to beginning of given string if it does not starts with it
        /// </summary>
        public static string EnsureStartsWith(this string str, char prefix, StringComparison comparisonType = StringComparison.CurrentCulture) {
            return EnsureStartsWith(str, prefix.ToString(), comparisonType);
        }

        /// <summary>
        ///     Adds a string to beginning of given string if it does not starts with it
        /// </summary>
        public static string EnsureStartsWith(this string str, string prefix, StringComparison comparisonType = StringComparison.CurrentCulture) {
            return str.StartsWith(prefix, comparisonType) ? str : (prefix + str);
        }

        /// <summary>
        ///     Adds a string to end of given string if it does not ends with it
        /// </summary>
        public static string EnsureEndsWith(this string str, char postfix, StringComparison comparisonType = StringComparison.CurrentCulture) {
            return EnsureEndsWith(str, postfix.ToString(), comparisonType);
        }

        /// <summary>
        ///     Adds a string to end of given string if it does not ends with it
        /// </summary>
        public static string EnsureEndsWith(this string str, string postfix, StringComparison comparisonType = StringComparison.CurrentCulture) {
            return str.EndsWith(postfix, comparisonType) ? str : (str + postfix);
        }

        /// <summary>
        ///     Return true if the string is an email address
        /// </summary>
        public static bool IsEmail(this string value) {
            return !string.IsNullOrEmpty(value) && Regex.IsMatch(value, EmailRegex);
        }

        /// <summary>
        ///     Used by <see cref="IsEmail(string)"/> to check a string represents an email address
        /// </summary>
        public const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
    }

}