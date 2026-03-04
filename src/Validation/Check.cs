using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GPSoftware.Core.Validation {

    [DebuggerStepThrough]
    public static class Check {

        /// <summary>
        ///     Check the passed value is not null and if it is, throw an exception with a default or optionally passed message.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static T NotNull<T>(
            T value,
            string? parameterName,
            string? message = null) {
            if (object.Equals(value, default(T))) {
                throw new ArgumentNullException(parameterName, message ?? $"{parameterName ?? "parameter"} can not be null!");
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null and has a length in the passed range. if not, throw an exception 
        ///     with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static string NotNull(
            string? value,
            string? parameterName,
            int maxLength = int.MaxValue,
            int minLength = 0,
            string? message = null) {
            if (value == null) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} can not be null!", parameterName);
            }

            if (value.Length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or lower than {maxLength}!", parameterName);
            }

            if (minLength > 0 && value.Length < minLength) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or bigger than {minLength}!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null or set with a default value.
        ///     if not, throw an exception with a default or optionally passed message.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static T NotNullOrDefault<T>(
            T? value,
            string? parameterName,
            string? message = null)
            where T : struct {

            if (value == null) {
                throw new ArgumentException(message ?? $"{parameterName} is null!", parameterName);
            }

            if (value.Value.Equals(default(T))) {
                throw new ArgumentException(message ?? $"{parameterName} has a default value!", parameterName);
            }

            return value.Value;
        }

        /// <summary>
        ///     Check the passed value is not null or only with white spaces and has a length in the passed range.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static string NotNullOrWhiteSpace(
            string? value,
            string? parameterName,
            int maxLength = int.MaxValue,
            int minLength = 0,
            string? message = null) {
            if (string.IsNullOrWhiteSpace(value)) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} can not be null, empty or white space!", parameterName);
            }

            if (value!.Length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or lower than {maxLength}!", parameterName);
            }

            if (minLength > 0 && value.Length < minLength) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or bigger than {minLength}!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null or empty and has a length in the passed range.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static string NotNullOrEmpty(
            string? value,
            string? parameterName,
            int maxLength = int.MaxValue,
            int minLength = 0,
            string? message = null) {
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} can not be null or empty!", parameterName);
            }

            if (value!.Length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or lower than {maxLength}!", parameterName);
            }

            if (minLength > 0 && value.Length < minLength) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or bigger than {minLength}!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed collection is not null or empty.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static ICollection<T> NotNullOrEmpty<T>(
            ICollection<T> value,
            string? parameterName,
            string? message = null) {
            if ((value?.Count ?? 0) == 0) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} can not be null or empty!", parameterName);
            }

            return value!;
        }

        /// <summary>
        ///     Check the passed comparable object (int, long, Datetime, etc.) is null or out of the passed min/max values.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public static T Range<T>(
            T value,
            T minValue,
            T maxValue,
            string? parameterName,
            string? message = null)
            where T : IComparable<T> {
            if (object.Equals(value, default(T))) {
                throw new ArgumentNullException(message ?? $"{parameterName ?? "parameter"} can not be null!", parameterName);
            }

            if ((value.CompareTo(minValue) < 0) || (value.CompareTo(maxValue) > 0)) {
                throw new ArgumentOutOfRangeException(message ?? $"{parameterName ?? "parameter"} is out of the passed range!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Validates that the specified collection contains a number of elements within the given minimum and maximum bounds.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection to validate.</typeparam>
        /// <param name="value">The collection whose length is to be validated. Cannot be null.</param>
        /// <param name="minLength">The minimum number of elements the collection must contain.</param>
        /// <param name="maxLength">The maximum number of elements the collection can contain.</param>
        /// <param name="parameterName">The name of the parameter representing the collection, used in exception messages.</param>
        /// <param name="message">An optional custom message to include in the exception if validation fails.</param>
        /// <returns>The original collection if its length is within the specified bounds.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the number of elements in the collection is less than the specified minimum
        ///     or greater than the specified maximum.
        /// </exception>
        public static IEnumerable<T> Length<T>(
            IEnumerable<T> value,
            int minLength,
            int maxLength,
            string? parameterName,
            string? message = null) {

            Check.NotNull(value, parameterName, message);

            var length = value.Count();

            if (!((minLength <= length) && (length <= maxLength))) {
                throw new ArgumentException(message ?? $"{parameterName ?? "parameter"} length must be equal to or lower than {maxLength}!", parameterName);
            }

            return value;
        }
    }
}
