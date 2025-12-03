using System.Globalization;

namespace GPSoftware.Core.Extensions {

    /// <summary>
    ///     Generic extension methods for <see cref = "CultureInfo" />
    /// </summary>
    public static class CultureInfoExtension {

        /// <summary>
        ///     Returns the name of the culture associated with the current TextInfo
        ///     The name is in format like "en-US"
        /// </summary>
        public static string CultureName(this CultureInfo culture) {
            return culture.TextInfo.CultureName;
        }

        /// <summary>
        ///     Returns the full name of the CultureInfo. The name is in format like "en_US"
        /// </summary>
        public static string FourLetterName(this CultureInfo culture) {
            return culture.CultureName().Replace('-', '_');
        }
    }
}