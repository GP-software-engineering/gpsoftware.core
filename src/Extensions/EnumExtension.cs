using System;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref="Enum"/>
    /// </summary>
    public static class EnumExtension {
        //public static TEnum GetNextValue<TEnum>(this TEnum value) {
        //    if (typeof(TEnum).BaseType != typeof(Enum)) {
        //        throw new ArgumentException("Passed byte must be an Enum");
        //    }
        //    var nextValue = Enum.GetValues(typeof(TEnum)).Cast<TEnum>()
        //        .SkipWhile(e => e != value).Skip(1).First();
        //}

        /// <summary>
        ///     Counting the number of flags set on an enumeration
        /// </summary>
        /// <remarks>Works for 32 bits</remarks>
        public static UInt32 CountFlags<TEnum>(this UInt32 flags) {
            if (typeof(TEnum).BaseType != typeof(Enum)) {
                throw new ArgumentException("Passed byte must be an Enum");
            }

            UInt32 v = flags;
            v = v - ((v >> 1) & 0x55555555); // reuse input as temporary
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333); // temp
            UInt32 c = ((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
            return c;
        }

        /// <summary>
        ///     Counting the number of flags set on an enumeration
        /// </summary>
        /// <remarks>Works for 32 bits</remarks>
        public static UInt32 CountFlags<TEnum>(this int flags) {
            return ((UInt32)flags).CountFlags<TEnum>();
        }

        /// <summary>
        ///     Counting the number of flags set on an enumeration
        /// </summary>
        /// <remarks>Works for 32 bits</remarks>
        public static UInt32 CountFlags<TEnum>(this byte flags) {
            return ((UInt32)flags).CountFlags<TEnum>();
        }

        /// <summary>
        /// 
        /// </summary>
        public static TEnum? ConvertToType<TEnum>(this int value, TEnum? defaultValue = null) where TEnum : struct, IConvertible {
            return (value as int?).ConvertToType<TEnum>(defaultValue);
        }

        /// <summary>
        /// 
        /// </summary>
        public static TEnum? ConvertToType<TEnum>(this int? value, TEnum? defaultValue = null) where TEnum : struct, IConvertible {
            if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum must be an enumerated type");
            if (!value.HasValue) return defaultValue;
            return Enum.IsDefined(typeof(TEnum), value.Value) ? (TEnum)Enum.ToObject(typeof(TEnum), value.Value) : defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public static TEnum ConvertToType<TEnum>(this string value, TEnum defaultValue) where TEnum : struct, IConvertible {
            if (!typeof(TEnum).IsEnum) throw new ArgumentException("TEnum must be an enumerated type");
            if (string.IsNullOrEmpty(value)) return defaultValue;
            var valuez = value.Trim();
            foreach (TEnum item in Enum.GetValues(typeof(TEnum))) {
                if (valuez.Equals(item.ToString(), StringComparison.OrdinalIgnoreCase)) return item;
            }
            return defaultValue;
        }
    }
}