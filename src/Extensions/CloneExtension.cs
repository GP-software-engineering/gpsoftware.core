using System;
using System.Linq;
using GPSoftware.Core.Dtos;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods to clone simple objects (typically DAOs/POCOs)
    /// </summary>
    public static class CloneExtension {

        /// <summary>
        ///     Clone an object into a new object of the same type.
        /// </summary>
        public static TDto Clone<TDto>(this TDto source)
            where TDto : IMyDto, new() {
            return source.Clone(new TDto());
        }

        /// <summary>
        ///     Clone an object into a new object of another type. ONLY SAME properties will be copied.
        /// </summary>
        public static TDtoDest Clone<TDtoSource, TDtoDest>(this TDtoSource source)
            where TDtoSource : IMyDto
            where TDtoDest : IMyDto, new() {
            return source.Clone(new TDtoDest());
        }

        /// <summary>
        ///     Copy an object into a object of another type. ONLY SAME properties will be copied.
        ///     REMOVED 'new()' constraint here to allow recursion on Interfaces.
        /// </summary>
        public static TDtoDest Clone<TDtoSource, TDtoDest>(this TDtoSource source, TDtoDest dest)
            where TDtoSource : IMyDto
            where TDtoDest : IMyDto {

            var srcProperties = source.GetType().GetProperties();
            var destProperties = dest.GetType().GetProperties();
            foreach (var srcProp in srcProperties) {
                // Find matching property in destination
                var destPrp = destProperties.FirstOrDefault(dp => 
                    dp.CanWrite && 
                    dp.Name.Equals(srcProp.Name) && 
                    dp.PropertyType.IsAssignableFrom(srcProp.PropertyType)); // Slightly more robust check

                // Strict equality check (as in your original code)
                if (destPrp == null && srcProp.PropertyType.IsClass) {
                     destPrp = destProperties.FirstOrDefault(dp => 
                        dp.CanWrite && 
                        dp.Name.Equals(srcProp.Name) && 
                        dp.PropertyType.Equals(srcProp.PropertyType));
                }

                if (destPrp == null) continue;

                var srcValue = srcProp.GetValue(source);

                if (srcValue == null) {
                    destPrp.SetValue(dest, null);
                    continue;
                }

                // DEEP CLONE LOGIC
                // Check if the property is a DTO (implements IMyDto)
                if (srcValue is IMyDto srcDto) {
                    // Create a new instance of the concrete type of the destination property
                    // We can use Activator because we know the property type is a concrete class at runtime
                    var destDtoObj = Activator.CreateInstance(destPrp.PropertyType);
                    if (destDtoObj is not IMyDto destDto) {
                        throw new InvalidOperationException($"Property {destPrp.Name} is not of type IMyDto.");
                    }

                    // Recursive call. 
                    // This now works because the 3rd overload no longer requires 'new()' constraint on the interface types.
                    CloneExtension.Clone(srcDto, destDto);

                    destPrp.SetValue(dest, destDto);
                }
                else {
                    // Shallow copy for simple types / value types
                    destPrp.SetValue(dest, srcValue);
                }
            }
            return dest;
        }
    }
}
