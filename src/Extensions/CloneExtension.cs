using System;
using System.Linq;
using GPSoftware.Core.Dtos;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods to clone simple objects (typically DAOs/POCOs)
    /// </summary>
    public static class CloneExtension {

        /// <summary>
        ///     Clone an an object into a new object of the same type
        /// </summary>
        /// <returns></returns>
        public static TDto Clone<TDto>(this TDto source)
            where TDto : IMyDto, new() {
            return source.Clone(new TDto());
        }

        /// <summary>
        ///     Clone an an object into a new object of another type. ONLY SAME properties will be copied.
        /// </summary>
        public static TDtoDest Clone<TDtoSource, TDtoDest>(this TDtoSource source)
            where TDtoSource : IMyDto
            where TDtoDest : IMyDto, new() {
            return source.Clone(new TDtoDest());
        }

        /// <summary>
        ///     Copy an an object into a object of another type. ONLY SAME properties will be copied.
        /// </summary>
        public static TDtoDest Clone<TDtoSource, TDtoDest>(this TDtoSource source, TDtoDest dest)
            where TDtoSource : IMyDto
            where TDtoDest : IMyDto, new() {

            var srcProperties = source.GetType().GetProperties();
            var destProperties = dest.GetType().GetProperties();
            foreach (var srcProp in srcProperties) {
                var destPrp = destProperties.FirstOrDefault(dp => dp.CanWrite && srcProp.Name.Equals(dp.Name) && srcProp.PropertyType.Equals(dp.PropertyType));
                if (destPrp == null) continue;

                var srcValue = srcProp.GetValue(source);

                // DEEP CLONE LOGIC
                // If the value is not null and implements IMyDto, we must clone it recursively
                if (srcValue is IMyDto srcDto) {
                    // Create a new instance of the property type
                    var destDto = (IMyDto)Activator.CreateInstance(destPrp.PropertyType);
                    // Recursive call using the existing logic
                    CloneExtension.Clone(srcDto, destDto);                    
                    destPrp.SetValue(dest, destDto);
                } 
                else {
                    // Shallow copy for simple types (int, string, etc.)
                    destPrp.SetValue(dest, srcValue);
                }
            }
            return dest;
        }
    }
}
