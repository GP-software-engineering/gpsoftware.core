namespace GPSoftware.Core.Crypto {

    /// <summary>
    /// Provides a mechanism to encrypt and decrypt data.
    /// </summary>
    public interface IEncryptionProvider {

        /// <summary>
        ///     Encrypts the given input byte array and return a never null (possibly empty) encrypted array
        /// </summary>
        /// <param name="input">Input to encrypt.</param>
        /// <returns>A never null but possibly empty encrypted input</returns>
        byte[] Encrypt(byte[] input);

        /// <summary>
        ///     Encrypts the given input string and return a never null (possibly empty) encrypted array
        /// </summary>
        /// <param name="input">Input to encrypt.</param>
        /// <returns>A never null but possibly empty encrypted input</returns>
        byte[] Encrypt(string? input);

        /// <summary>
        ///     Encrypts the given input byte array and return its possibly null base64 encoded string
        /// </summary>
        /// <param name="input">Input to encrypt.</param>
        /// <returns>A possibly null encrypted input</returns>
        string? EncryptToBase64(byte[] input);

        /// <summary>
        ///     Encrypts the given input string and return its possibly null base64 encrypted string
        /// </summary>
        /// <param name="input">Input to encrypt.</param>
        /// <returns>A possibly null encrypted input</returns>
        string? EncryptToBase64(string input);

        /// <summary>
        ///     Decrypts the given input byte array.
        /// </summary>
        /// <param name="input">Input to decrypt.</param>
        /// <returns>Decrypted input.</returns>
        byte[] Decrypt(byte[] input);

        /// <summary>
        ///     Decrypts the given base64-encoded encrypted string and returns a possibly null decrypted string.
        /// </summary>
        /// <param name="input">Input to decrypt. If null, return a null string</param>
        /// <returns>Decrypted input.</returns>
        string? DecryptFromBase64(string? input);
    }
}
