using System.Security.Cryptography;
using System.Text;

namespace GPSoftware.Core.Crypto {

    public static class Hasher {

        /// <summary>
        ///     Compute a SHA 1 hash onto a string
        /// </summary>
        public static string ComputeSha1Hash(string rawData) {
            using (var shaHash = SHA1.Create()) {
                return ComputeShaHash(rawData, shaHash);
            }
        }

        /// <summary>
        ///     Compute a SHA 256 hash onto a string
        /// </summary>
        public static string ComputeSha256Hash(string rawData) {
            using (var shaHash = SHA256.Create()) {
                return ComputeShaHash(rawData, shaHash);
            }
        }

        /// <summary>
        ///     Compute a SHA hash onto a string using the passed Hash algorithm
        /// </summary>
        private static string ComputeShaHash(string rawData, HashAlgorithm shaHash) {
            // ComputeHash - returns byte array  
            byte[] bytes = shaHash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++) {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
