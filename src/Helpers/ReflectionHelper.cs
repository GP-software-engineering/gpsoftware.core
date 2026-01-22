using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GPSoftware.Core.Helpers {

    /// <summary>
    ///     Helper class for common Reflection operations.
    /// </summary>
    public static class ReflectionHelper {

        /// <summary>
        ///     Checks whether a <paramref name="givenType"/> implements or inherits the passed <paramref name="genericType"/>.
        /// </summary>
        /// <param name="givenType">The type to check.</param>
        /// <param name="genericType">The generic type definition to verify against.</param>
        /// <returns>True if the type is assignable to the generic type; otherwise, false.</returns>
        public static bool IsAssignableToGenericType(Type givenType, Type genericType) {
            var givenTypeInfo = givenType.GetTypeInfo();

            if (givenTypeInfo.IsGenericType && givenType.GetGenericTypeDefinition() == genericType) {
                return true;
            }

            foreach (var interfaceType in givenType.GetInterfaces()) {
                if (interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType) {
                    return true;
                }
            }

            if (givenTypeInfo.BaseType == null) {
                return false;
            }

            return IsAssignableToGenericType(givenTypeInfo.BaseType, genericType);
        }

        /// <summary>
        ///     Gets a list of attributes defined for a class member and a specific type, optionally including inherited attributes.
        /// </summary>
        /// <param name="memberInfo">The member metadata.</param>
        /// <param name="type">The type to check for attributes.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>A list of attributes found.</returns>
        public static List<object> GetAttributesOfMemberAndType(MemberInfo memberInfo, Type type, bool inherit = true) {
            var attributeList = new List<object>();
            attributeList.AddRange(memberInfo.GetCustomAttributes(inherit));
            attributeList.AddRange(type.GetTypeInfo().GetCustomAttributes(inherit));
            return attributeList;
        }

        /// <summary>
        ///     Gets a list of attributes of a specific type defined for a class member and a specific type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to search for.</typeparam>
        /// <param name="memberInfo">The member metadata.</param>
        /// <param name="type">The type to check for attributes.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>A strongly typed list of attributes found.</returns>
        public static List<TAttribute> GetAttributesOfMemberAndType<TAttribute>(MemberInfo memberInfo, Type type, bool inherit = true)
            where TAttribute : Attribute {
            var attributeList = new List<TAttribute>();

            if (memberInfo.IsDefined(typeof(TAttribute), inherit)) {
                attributeList.AddRange(memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>());
            }

            if (type.GetTypeInfo().IsDefined(typeof(TAttribute), inherit)) {
                attributeList.AddRange(type.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>());
            }

            return attributeList;
        }

        /// <summary>
        ///     Gets a list of attributes defined for a class member and its declaring type, optionally including inherited attributes.
        /// </summary>
        /// <param name="memberInfo">The member metadata.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>A list of attributes found.</returns>
        public static List<object> GetAttributesOfMemberAndDeclaringType(MemberInfo memberInfo, bool inherit = true) {
            var attributeList = new List<object>();

            attributeList.AddRange(memberInfo.GetCustomAttributes(inherit));

            // Check if the member is declared in a type (it usually is, unless it's a module member)
            if (memberInfo.DeclaringType != null) {
                attributeList.AddRange(memberInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(inherit));
            }

            return attributeList;
        }

        /// <summary>
        ///     Gets a list of attributes of a specific type defined for a class member and its declaring type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to search for.</typeparam>
        /// <param name="memberInfo">The member metadata.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>A strongly typed list of attributes found.</returns>
        public static List<TAttribute> GetAttributesOfMemberAndDeclaringType<TAttribute>(MemberInfo memberInfo, bool inherit = true)
            where TAttribute : Attribute {
            var attributeList = new List<TAttribute>();

            if (memberInfo.IsDefined(typeof(TAttribute), inherit)) {
                attributeList.AddRange(memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>());
            }

            if (memberInfo.DeclaringType != null && memberInfo.DeclaringType.GetTypeInfo().IsDefined(typeof(TAttribute), inherit)) {
                attributeList.AddRange(memberInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>());
            }

            return attributeList;
        }

        /// <summary>
        ///     Tries to get a single attribute of a specific type defined for a member or its declaring type.
        ///     Returns the default value if not found.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to search for.</typeparam>
        /// <param name="memberInfo">The member metadata.</param>
        /// <param name="defaultValue">The value to return if no attribute is found.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>The found attribute or the default value.</returns>
        public static TAttribute? GetSingleAttributeOfMemberOrDeclaringTypeOrDefault<TAttribute>(MemberInfo memberInfo, TAttribute? defaultValue = default, bool inherit = true)
            where TAttribute : Attribute {

            // Check attribute on the member itself
            var attr = memberInfo.GetCustomAttributes(inherit).OfType<TAttribute>().FirstOrDefault();
            if (attr != null) {
                return attr;
            }

            // Check attribute on the declaring type
            // Note: ReflectedType might differ from DeclaringType in inheritance scenarios, 
            // but for attributes DeclaringType is usually the intended target.
            // Using MemberInfo.ReflectedType might return null in .NET Core in some cases.
            if (memberInfo.DeclaringType != null) {
                var typeAttr = memberInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(inherit).OfType<TAttribute>().FirstOrDefault();
                if (typeAttr != null) {
                    return typeAttr;
                }
            }

            return defaultValue;
        }

        /// <summary>
        ///     Tries to get a single attribute of a specific type defined for a member.
        ///     Returns the default value if not found.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to search for.</typeparam>
        /// <param name="memberInfo">The member metadata.</param>
        /// <param name="defaultValue">The value to return if no attribute is found.</param>
        /// <param name="inherit">Whether to search the inheritance chain.</param>
        /// <returns>The found attribute or the default value.</returns>
        public static TAttribute? GetSingleAttributeOrDefault<TAttribute>(MemberInfo memberInfo, TAttribute? defaultValue = default, bool inherit = true)
            where TAttribute : Attribute {
            if (memberInfo.IsDefined(typeof(TAttribute), inherit)) {
                return memberInfo.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>().First();
            }

            return defaultValue;
        }

        /// <summary>
        ///     Gets a <see cref="PropertyInfo"/> by its full dot-separated path from a given object type.
        /// </summary>
        /// <param name="obj">The object instance (used for initial type resolution if needed).</param>
        /// <param name="objectType">The type of the root object.</param>
        /// <param name="propertyPath">The full dot-separated path of the property (e.g. "Customer.Address.City").</param>
        /// <returns>The value of the property found at the end of the path.</returns>
        /// <remarks>
        ///     The method name suggests it returns a PropertyInfo, but the implementation logic traverses values ("obj") as well.
        ///     Refactored to be consistent with 'GetValueByPath' logic but returns the *value* of the property.
        ///     If you strictly need PropertyInfo, the logic would need to change to avoid 'GetValue'.
        /// </remarks>
        public static object? GetPropertyByPath(object obj, Type objectType, string propertyPath) {
            // Note: Original implementation was functionally identical to GetValueByPath.
            // Kept as is to preserve existing behavior, but effectively it retrieves the Value.
            return GetValueByPath(obj, objectType, propertyPath);
        }

        /// <summary>
        ///     Gets the value of a property by its full dot-separated path.
        /// </summary>
        /// <param name="obj">The root object instance.</param>
        /// <param name="objectType">The type of the root object.</param>
        /// <param name="propertyPath">The full path to the property.</param>
        /// <returns>The value of the property.</returns>
        public static object? GetValueByPath(object? obj, Type objectType, string propertyPath) {
            if (obj == null) return null;

            string cleanPath = SanitizePropertyPath(objectType, propertyPath);
            var currentObj = obj;
            var currentType = objectType;

            foreach (var propertyName in cleanPath.Split('.')) {
                var property = currentType.GetProperty(propertyName);
                if (property == null) {
                    throw new ArgumentException($"Property '{propertyName}' not found on type '{currentType.FullName}'");
                }

                currentObj = property.GetValue(currentObj, null);
                if (currentObj == null) {
                    return null; // Stop navigation if a midway property is null
                }
                currentType = property.PropertyType;
            }

            return currentObj;
        }

        /// <summary>
        ///     Sets the value of a property by its full dot-separated path on a given object.
        /// </summary>
        /// <param name="obj">The root object instance.</param>
        /// <param name="objectType">The type of the root object.</param>
        /// <param name="propertyPath">The full path to the property.</param>
        /// <param name="value">The value to set.</param>
        public static void SetValueByPath(object obj, Type objectType, string propertyPath, object? value) {
            string cleanPath = SanitizePropertyPath(objectType, propertyPath);
            var properties = cleanPath.Split('.');

            var currentObj = obj;
            var currentType = objectType;

            // Traverse down to the penultimate property (the parent of the target property)
            for (int i = 0; i < properties.Length - 1; i++) {
                var propInfo = currentType.GetProperty(properties[i]);
                if (propInfo == null) {
                    throw new ArgumentException($"Property '{properties[i]}' not found on type '{currentType.FullName}'");
                }

                currentObj = propInfo.GetValue(currentObj, null);
                if (currentObj == null) {
                    throw new InvalidOperationException($"Property '{properties[i]}' is null. Cannot set nested property '{properties[i + 1]}'.");
                }
                currentType = propInfo.PropertyType;
            }

            // Set value on the final property
            var finalPropertyName = properties.Last();
            var finalProp = currentType.GetProperty(finalPropertyName);
            if (finalProp == null) {
                throw new ArgumentException($"Property '{finalPropertyName}' not found on type '{currentType.FullName}'");
            }

            if (!finalProp.CanWrite) {
                throw new InvalidOperationException($"Property '{finalPropertyName}' on type '{currentType.FullName}' is read-only.");
            }

            finalProp.SetValue(currentObj, value);
        }

        /// <summary>
        ///     Checks if a method is a property getter or setter.
        /// </summary>
        public static bool IsPropertyGetterSetterMethod(MethodInfo method, Type type) {
            if (!method.IsSpecialName) {
                return false;
            }

            if (method.Name.Length < 5) { // Needs at least "get_X" or "set_X"
                return false;
            }

            string propName = method.Name.Substring(4);

            // FIX S3011: Safe bypass. This is a Reflection helper intended to inspect class structures,
            // including private properties backing these methods.
#pragma warning disable S3011
            return type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public) != null;
#pragma warning restore S3011
        }

        /// <summary>
        ///     Invokes a method asynchronously and awaits its result.
        /// </summary>
        /// <param name="method">The method to invoke.</param>
        /// <param name="obj">The object instance to invoke on (null for static methods).</param>
        /// <param name="parameters">The method parameters.</param>
        /// <returns>The result of the async task, or null if it returns void/Task.</returns>
        public static async Task<object?> InvokeAsync(MethodInfo method, object? obj, params object[] parameters) {
            var taskObject = method.Invoke(obj, parameters);

            if (taskObject is Task task) {
                await task.ConfigureAwait(false);

                // Check if it's a generic Task<T>
                var taskType = task.GetType();
                if (taskType.GetTypeInfo().IsGenericType && taskType.GetGenericTypeDefinition() == typeof(Task<>)) {
                    var resultProperty = taskType.GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
            }
            // Add support for ValueTask if project targets .NET Core / Standard 2.1+
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (taskObject is ValueTask valueTask) {
                await valueTask.ConfigureAwait(false);
                return null;
            } else if (taskObject != null) {
                // Handling generic ValueTask<T> requires a bit more dynamic work or specific type checking
                // Basic implementation treats unknown awaitables as sync result or null
            }
#endif

            return null;
        }

        /// <summary>
        ///     Removes the class name prefix from the property path if present.
        /// </summary>
        private static string SanitizePropertyPath(Type type, string propertyPath) {
            var objectPath = type.FullName;
            if (!string.IsNullOrEmpty(objectPath) && propertyPath.StartsWith(objectPath + ".")) {
                return propertyPath.Substring(objectPath.Length + 1);
            }
            return propertyPath;
        }
    }
}
