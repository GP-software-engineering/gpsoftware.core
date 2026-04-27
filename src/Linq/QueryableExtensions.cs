using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GPSoftware.Core.Validation;


namespace GPSoftware.Core.Linq {

    /// <summary>
    ///     Some useful extension methods for <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class QueryableExtensions {

        /// <summary>
        ///     Applies a flat search filter on all properties marked with [FlatSearchAllowedAttribute].
        ///     Uses Expression Trees to ensure compatibility with EF Core translation to SQL.
        /// </summary>
        public static IQueryable<T> ApplyFlatSearch<T>(this IQueryable<T> query, string searchTerm) {
            Check.NotNull(query, nameof(query));
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            // Convert search term to lowercase immediately
            searchTerm = searchTerm.ToLower();

            var flatProps = typeof(T).GetProperties()
                .Where(prop => Attribute.IsDefined(prop, typeof(FlatSearchAllowedAttribute)))
                .ToList();
            if (!flatProps.Any()) return query;

            var parameter = Expression.Parameter(typeof(T), "e");
            var searchTermExpression = Expression.Constant(searchTerm, typeof(string));
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes);

            Expression? combinedExpression = null;
            foreach (var prop in flatProps) {
                Expression propExpression = Expression.Property(parameter, prop);
                Expression stringExpression;

                if (prop.PropertyType == typeof(string)) {
                    stringExpression = propExpression;
                } else {
                    var toStringMethod = prop.PropertyType.GetMethod(nameof(object.ToString), Type.EmptyTypes) ?? typeof(object).GetMethod(nameof(object.ToString));
                    stringExpression = Expression.Call(propExpression, toStringMethod);
                }

                // Ensure we don't call .ToLower() or .Contains() on a null value
                var nullCheck = Expression.NotEqual(propExpression, Expression.Constant(null, prop.PropertyType));

                // stringExpression.ToLower()
                var toLowerExpression = Expression.Call(stringExpression, toLowerMethod);

                // stringExpression.ToLower().Contains(searchTerm)
                var containsExpression = Expression.Call(toLowerExpression, containsMethod, searchTermExpression);

                // e.Property != null && stringExpression.ToLower().Contains(searchTerm)
                var safeContains = Expression.AndAlso(nullCheck, containsExpression);

                if (combinedExpression == null) {
                    combinedExpression = safeContains;
                } else {
                    combinedExpression = Expression.OrElse(combinedExpression, safeContains);
                }
            }

            if (combinedExpression == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            return query.Where(lambda);
        }

        /// <summary>
        /// Used for paging. Can be used as an alternative to Skip(...).Take(...) chaining.
        /// </summary>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int maxResultCount) {
            Check.NotNull(query, nameof(query));

            return query.Skip(skipCount).Take(maxResultCount);
        }

        /// <summary>
        /// Used for paging. Can be used as an alternative to Skip(...).Take(...) chaining.
        /// </summary>
        public static TQueryable PageBy<T, TQueryable>(this TQueryable query, int skipCount, int maxResultCount)
            where TQueryable : IQueryable<T> {
            Check.NotNull(query, nameof(query));

            return (TQueryable)query.Skip(skipCount).Take(maxResultCount);
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="condition">A boolean value</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate) {
            Check.NotNull(query, nameof(query));

            return condition
                ? query.Where(predicate)
                : query;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="condition">A boolean value</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, bool condition, Expression<Func<T, bool>> predicate)
            where TQueryable : IQueryable<T> {
            Check.NotNull(query, nameof(query));

            return condition
                ? (TQueryable)query.Where(predicate)
                : query;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="condition">A boolean value</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, int, bool>> predicate) {
            Check.NotNull(query, nameof(query));

            return condition
                ? query.Where(predicate)
                : query;
        }

        /// <summary>
        /// Filters a <see cref="IQueryable{T}"/> by given predicate if given condition is true.
        /// </summary>
        /// <param name="query">Queryable to apply filtering</param>
        /// <param name="condition">A boolean value</param>
        /// <param name="predicate">Predicate to filter the query</param>
        /// <returns>Filtered or not filtered query based on <paramref name="condition"/></returns>
        public static TQueryable WhereIf<T, TQueryable>(this TQueryable query, bool condition, Expression<Func<T, int, bool>> predicate)
            where TQueryable : IQueryable<T> {
            Check.NotNull(query, nameof(query));

            return condition
                ? (TQueryable)query.Where(predicate)
                : query;
        }

        /// <summary>
        ///     Sum of TimeSpan
        /// </summary>
        public static TimeSpan Sum<T>(this IEnumerable<T> source, Func<T, TimeSpan> selector) {
            return source.Select(selector).Aggregate(TimeSpan.Zero, (t1, t2) => t1 + t2);
        }
    }
}
