using System;
using System.Security.Cryptography;
using System.Text;

namespace GPSoftware.Core.Crypto {

    /// <summary>
    ///     Provides helper methods and extensions for computing hashes.
    /// </summary>
    public static class Hasher {

        /// <summary>
        ///     Compute a SHA 1 hash onto a string.
        ///     Warning: SHA1 is not considered secure against collision attacks. Use SHA256 where possible.
        /// </summary>
        public static string ComputeSha1Hash(string rawData) {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;

            byte[] sourceBytes = Encoding.UTF8.GetBytes(rawData);

#if NET6_0_OR_GREATER
        byte[] hashBytes = SHA1.HashData(sourceBytes);
        return Convert.ToHexString(hashBytes); // Returns uppercase hex
#else
            // Legacy .NET Standard 2.0
            using (var sha = SHA1.Create()) {
                byte[] hashBytes = sha.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
#endif
        }

        /// <summary>
        ///     Compute a SHA 256 hash onto a string.
        /// </summary>
        public static string ComputeSha256Hash(string rawData) {
            if (string.IsNullOrEmpty(rawData)) return string.Empty;

            byte[] sourceBytes = Encoding.UTF8.GetBytes(rawData);

#if NET6_0_OR_GREATER
        // Modern .NET: Use static HashData for zero-allocation efficiency
        byte[] hashBytes = SHA256.HashData(sourceBytes);
        return Convert.ToHexString(hashBytes); // Returns uppercase hex
#else
            // Legacy .NET Standard 2.0
            using (var sha = SHA256.Create()) {
                byte[] hashBytes = sha.ComputeHash(sourceBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
#endif
        }


        #region Extensions

        /// <summary>
        ///     Extension method to compute SHA256 hash directly on a string.
        /// </summary>
        public static string ToSha256(this string rawData) {
            return ComputeSha256Hash(rawData);
        }

        /// <summary>
        ///     Extension method to compute SHA1 hash directly on a string.
        /// </summary>
        public static string ToSha1(this string rawData) {
            return ComputeSha1Hash(rawData);
        }

        #endregion
    }
}
