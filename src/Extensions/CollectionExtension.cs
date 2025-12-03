using System;
using System.Collections.Generic;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for Collections />
    /// </summary>
    public static class CollectionExtension {

        /// <summary>
        ///     Checks whatever given array of object is null or has no item.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this T[] source) {
            return source == null || source.Length <= 0;
        }

        /// <summary>
        ///     Checks whatever given array of object is null or has no item.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> source) {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        ///     Checks whatever given array of object is null or has no item.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source) {
            return source == null || source.Count <= 0;
        }

        /// <summary>
        ///     Checks whatever given array of object is null or has no item.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this List<T> source) {
            return ((ICollection<T>)source).IsNullOrEmpty();
        }

        /// <summary>
        ///     Adds an item to the collection if it's not already in the collection.
        /// </summary>
        /// <param name="source">Collection</param>
        /// <param name="item">Item to check and add</param>
        /// <typeparam name="T">Type of the items in the collection</typeparam>
        /// <returns>Returns True if added, returns False if not.</returns>
        public static bool AddIfNotContains<T>(this ICollection<T> source, T item) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Contains(item)) return false;
            source.Add(item);
            return true;
        }
    }
}