using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GPSoftware.Core.Validation {

    [DebuggerStepThrough]
    public static class Check {

        /// <summary>
        ///     Check the passed value is not null and if it is, throw an exception with a default or optionally passed message.
        /// </summary>
        /// <exception cref="ArgumentNullException">if the value is null</exception>
        public static T NotNull<T>(
            [NotNull] T? value,
            string parameterName,
            string? message = null) {
            
            if (value is null) {
                throw new ArgumentNullException(parameterName, message ?? $"{parameterName} cannot be null!");
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null and has a length in the passed range. if not, throw an exception 
        ///     with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentNullException">if the value is null</exception>
        /// <exception cref="ArgumentException">if the value length is out of the passed range</exception>
        public static string NotNull(
            [NotNull] string? value,
            string parameterName,
            int maxLength = int.MaxValue,
            int minLength = 0,
            string? message = null) {
            
            if (value is null) {
                throw new ArgumentNullException(parameterName, message ?? $"{parameterName} cannot be null!");
            }

            if (value.Length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName} length must be equal to or lower than {maxLength}!", parameterName);
            }

            if (minLength > 0 && value.Length < minLength) {
                throw new ArgumentException(message ?? $"{parameterName} length must be equal to or greater than {minLength}!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null or set with a default value.
        ///     if not, throw an exception with a default or optionally passed message.
        /// </summary>
        /// <exception cref="ArgumentNullException">if the value is null</exception>
        /// <exception cref="ArgumentException">if the value is set with a default value</exception>
        public static T NotNullOrDefault<T>(
            [NotNull] T? value,
            string parameterName,
            string? message = null)
            where T : struct {

            if (!value.HasValue) {
                throw new ArgumentNullException(parameterName, message ?? $"{parameterName} is null!");
            }

            if (EqualityComparer<T>.Default.Equals(value.Value, default)) {
                throw new ArgumentException(message ?? $"{parameterName} has a default value!", parameterName);
            }

            return value.Value;
        }

        /// <summary>
        ///     Checks if the passed Guid is not empty.
        /// </summary>
        /// <exception cref="ArgumentException">if the value is an empty Guid</exception>
        public static Guid NotDefault(
            Guid value,
            string parameterName,
            string? message = null) {

            if (value == Guid.Empty) {
                throw new ArgumentException(message ?? $"{parameterName} cannot be an empty Guid!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null or only with white spaces and has a length in the passed range.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentException">if the value is null, empty or with white spaces, only with white spaces</exception>
        public static string NotNullOrWhiteSpace(
            [NotNull] string? value,
            string parameterName,
            int maxLength = int.MaxValue,
            int minLength = 0,
            string? message = null) {
            
            if (string.IsNullOrWhiteSpace(value)) {
                throw new ArgumentException(message ?? $"{parameterName} cannot be null, empty or white space!", parameterName);
            }

            if (value!.Length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName} length must be equal to or lower than {maxLength}!", parameterName);
            }

            if (minLength > 0 && value.Length < minLength) {
                throw new ArgumentException(message ?? $"{parameterName} length must be equal to or greater than {minLength}!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed value is not null or empty and has a length in the passed range.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentException">if the value is null or empty</exception>
        public static string NotNullOrEmpty(
            [NotNull] string? value,
            string parameterName,
            int maxLength = int.MaxValue,
            int minLength = 0,
            string? message = null) {
            
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException(message ?? $"{parameterName} cannot be null or empty!", parameterName);
            }

            if (value!.Length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName} length must be equal to or lower than {maxLength}!", parameterName);
            }

            if (minLength > 0 && value.Length < minLength) {
                throw new ArgumentException(message ?? $"{parameterName} length must be equal to or greater than {minLength}!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed collection is not null or empty.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentException">if the value is null or empty</exception>
        public static ICollection<T> NotNullOrEmpty<T>(
            [NotNull] ICollection<T>? value,
            string parameterName,
            string? message = null) {
            
            if (value is null || value.Count == 0) {
                throw new ArgumentException(message ?? $"{parameterName} cannot be null or empty!", parameterName);
            }

            return value;
        }

        /// <summary>
        ///     Check the passed comparable object (int, long, Datetime, etc.) is null or out of the passed min/max values.
        ///     if not, throw an exception with a default or optionally passed message
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">if the value is null or out of the passed min/max values</exception>
        public static T Range<T>(
            T value,
            T minValue,
            T maxValue,
            string parameterName,
            string? message = null)
            where T : struct, IComparable<T> {
            
            if ((value.CompareTo(minValue) < 0) || (value.CompareTo(maxValue) > 0)) {
                throw new ArgumentOutOfRangeException(parameterName, value, message ?? $"{parameterName} is out of the allowed range [{minValue} - {maxValue}]!");
            }

            return value;
        }

        /// <summary>
        ///     Validates that the specified sequence contains a number of elements within the given bounds.
        ///     Optimized to prevent multiple enumerations.
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
                    IEnumerable<T>? value,
                    int minLength,
                    int maxLength,
                    string parameterName,
                    string? message = null) {

            Check.NotNull(value, parameterName, message);

            int length;

            // Highly optimized O(1) count extraction compatible with .NET Standard 2.0
            if (value is ICollection<T> genericCollection) {
                length = genericCollection.Count;
            } else if (value is IReadOnlyCollection<T> readOnlyCollection) {
                length = readOnlyCollection.Count;
            } else if (value is System.Collections.ICollection collection) {
                length = collection.Count;
            } else {
                // O(N) fallback: iterates the sequence only if it's a pure IEnumerable (e.g., yielded results)
                length = value.Count();
            }

            if (length < minLength || length > maxLength) {
                throw new ArgumentException(message ?? $"{parameterName} length ({length}) must be between {minLength} and {maxLength}!", parameterName);
            }

            return value;
        }
    }
}
