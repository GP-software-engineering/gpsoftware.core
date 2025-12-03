using System;
using System.Linq;
using System.Reflection;
using GPSoftware.Core.Linq;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref="Type"/>
    /// </summary>
    public static class TypeExtension {

        // Cache the simple types to avoid array allocation on every call
        private static readonly Type[] s_simpleTypes = new Type[] {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
        };

        /// <summary>
        ///     Determine whether a type is simple (String, Decimal, DateTime, etc) 
        ///     or complex (i.e. custom class with public properties and methods).
        /// </summary>
        /// <see cref="http://stackoverflow.com/questions/2442534/how-to-test-if-type-is-primitive"/>
        public static bool IsSimpleType(this Type type) {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                s_simpleTypes.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        /// <summary>
        ///     Returns the properties of a type that are marked with the attribute <see cref="FlatSearchAllowedAttribute"/>
        /// </summary>
        public static PropertyInfo[] GetFlatSearchProperties(this Type type) {
            return GetFlatSearchProperties(type, null);
        }

        /// <summary>
        ///     Returns the properties of a type that are marked with the attribute <see cref="FlatSearchAllowedAttribute"/>,
        ///     including the property named Id, if present
        /// </summary>
        public static PropertyInfo[] GetFlatSearchPropertiesWithId(this Type type) {
            var idProp = type.GetProperty("Id");
            // Pass the Id property only if it exists
            return idProp != null 
                ? GetFlatSearchProperties(type, idProp) 
                : GetFlatSearchProperties(type, null);
        }

        /// <summary>
        ///     Returns the properties of a type that are marked with the attribute <see cref="FlatSearchAllowedAttribute"/>
        ///     It also add the passed properties, even if they are not marked with the attribute.
        /// </summary>
        public static PropertyInfo[] GetFlatSearchProperties(this Type type, params PropertyInfo[]? additionalProperties) {
            var props = type.GetProperties()
                            .Where(prop => Attribute.IsDefined(prop, typeof(FlatSearchAllowedAttribute)))
                            .ToList();
            
            if (additionalProperties != null) {
                foreach (var prop in additionalProperties) {
                    if (prop != null) {
                         // Assumes AddIfNotContains is available in GPSoftware.Core.Linq
                        props.AddIfNotContains(prop);
                    }
                }
            }
            return props.ToArray();
        }
    }
}