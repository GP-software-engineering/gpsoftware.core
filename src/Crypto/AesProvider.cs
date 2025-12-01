using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GPSoftware.Core.Crypto {

    /// <summary>
    ///     Implements the Advanced Encryption Standard (AES) symmetric algorithm.
    ///     It is able to work both with a fixed initiation vector (IV) and with a random one, depending by the constructor invoked.
    /// </summary>
    /// <remarks>This class caches the aes encryptor to improve performances and it is thread-safe</remarks>
    public class AesProvider : IEncryptionProvider, IDisposable {

        /// <summary>
        ///     Default AES key size.
        /// </summary>
        const AesKeySize DefaultAesKeySize = AesKeySize.AES192Bits;

        /// <summary>
        ///     AES block size constant.
        /// </summary>
        public const int DefaultAesBlockSize = 128;

        /// <summary>
        ///     Initialization vector size constant.
        /// </summary>
        public const int DefaultInitializationVectorSize = 16;

        protected readonly Aes _aes;
        protected readonly byte[]? _initializationVector;

        protected static readonly object _mySynchObject = new object();

        /// <summary>
        ///     Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings and byte arrays.
        ///     Every encryption will uses the same key and initialization vector.
        /// </summary>
        /// <param name="key">AES key used for the symmetric encryption.</param>
        /// <param name="initializationVector">AES Initialization vector used for the symmetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        public AesProvider(
            byte[] key, 
            byte[] initializationVector, 
            CipherMode mode = CipherMode.CBC, 
            PaddingMode padding = PaddingMode.PKCS7) {
            if (key == null) throw new ArgumentNullException(nameof(key), "");
            if (initializationVector == null || initializationVector.Length <= 0) throw new ArgumentNullException(nameof(initializationVector), "");

            _initializationVector = new byte[initializationVector.Length];
            Buffer.BlockCopy(initializationVector, 0, _initializationVector, 0, initializationVector.Length);

            _aes = CreateCryptographyProvider(
                key: key,
                iv: _initializationVector,   // use the same IV for all new encryptions
                mode: mode,
                padding: padding,
                blockSize: DefaultAesBlockSize);
        }

        /// <summary>
        ///     Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings and byte arrays.
        ///     Every single encrypted string / byte array will use the same key and a random initiation vector.
        /// </summary>
        /// <param name="key">AES key used for the symmetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        public AesProvider(byte[] key, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7) {
            if (key == null) throw new ArgumentNullException(nameof(key), "");
            _aes = CreateCryptographyProvider(
                key: key,
                iv: null,   // use a random IV for each new encryption
                mode: mode,
                padding: padding,
                blockSize: DefaultAesBlockSize);

        }

        /// <summary>
        ///     Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings and byte arrays.
        ///     Every single encrypted string / byte array will use the same key and a random initialization vector.
        /// </summary>
        /// <param name="plainKey">AES key used for the symmetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        /// <param name="keySize">specifies the AES Key sizes used for generating the real encryption key.</param>
        public AesProvider(string plainKey, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, AesKeySize keySize = DefaultAesKeySize) {
            if (string.IsNullOrWhiteSpace(plainKey)) throw new ArgumentNullException(nameof(plainKey), "");
#if !NETSTANDARD2_0
            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(plainKey, DefaultInitializationVectorSize, 5000, HashAlgorithmName.SHA256);
#else
            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(plainKey, DefaultInitializationVectorSize, 5000);
#endif
            _aes = CreateCryptographyProvider(
                key: keyGenerator.GetBytes((int)((uint)keySize / 8)),
                iv: null,   // use a random IV for each new encryption
                mode: mode, 
                padding: padding,
                blockSize: DefaultAesBlockSize);
        }

        /// <summary>
        ///     Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings and byte arrays.
        ///     All encrypted string / byte array will use the same derived key and the same salt / initialization vector.
        /// </summary>
        /// <param name="plainKey">AES key used to derive the real encryption key for the symmetric encryption.</param>
        /// <param name="salt">The salt used to derive the real encryption key.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        /// <param name="keySize">specifies the AES Key sizes used for generating the real encryption key.</param>
        public AesProvider(string plainKey, string salt, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, AesKeySize keySize = DefaultAesKeySize)
            : this(plainKey, Encoding.UTF8.GetBytes(salt), mode, padding, keySize) {
        }

        /// <summary>
        ///     Creates a new <see cref="AesProvider"/> instance used to perform symmetric encryption and decryption on strings and byte arrays.
        ///     All encrypted string / byte array will use the same derived key and the same salt / initialization vector.
        /// </summary>
        /// <param name="plainKey">AES key used to derive the real encryption key for the symmetric encryption.</param>
        /// <param name="salt">The salt used to derive the real encryption key.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        /// <param name="keySize">specifies the AES Key sizes used for generating the real encryption key.</param>
        public AesProvider(string plainKey, byte[] salt, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7, AesKeySize keySize = DefaultAesKeySize) {
            if (string.IsNullOrWhiteSpace(plainKey)) throw new ArgumentNullException(nameof(plainKey), "");
            if (salt == null || salt.Length == 0) throw new ArgumentNullException(nameof(salt), "");

#if !NETSTANDARD2_0
            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(plainKey, salt, 5000, HashAlgorithmName.SHA256);
#else
            Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(plainKey, salt, 5000);
#endif
            _initializationVector = keyGenerator.GetBytes(DefaultAesBlockSize / 8);

            _aes = CreateCryptographyProvider(
                key: keyGenerator.GetBytes((int)((uint)keySize / 8)),
                iv: _initializationVector,   // use the same IV for all new encryptions
                mode: mode,
                padding: padding,
                blockSize: DefaultAesBlockSize);
        }

        /// <inheritdoc cref="IEncryptionProvider.EncryptToBase64(string)" />
        public virtual string? EncryptToBase64(string? input) {
            return string.IsNullOrEmpty(input) ? null : EncryptToBase64(Encoding.UTF8.GetBytes(input));
        }

        /// <inheritdoc cref="IEncryptionProvider.EncryptToBase64(byte[])" />
        public virtual string? EncryptToBase64(byte[] input) {
            var encryptedData = Encrypt(input);
            return (encryptedData == null || encryptedData.Length == 0) ? null : Convert.ToBase64String(encryptedData);
        }

        /// <inheritdoc cref="IEncryptionProvider.Encrypt(string)" />
        public virtual byte[] Encrypt(string? input) {
            return string.IsNullOrEmpty(input) ? new byte[0] : Encrypt(Encoding.UTF8.GetBytes(input));
        }

        /// <inheritdoc cref="IEncryptionProvider.Encrypt(byte[])" />
        public virtual byte[] Encrypt(byte[] input) {
            if (input is null || input.Length == 0) {
                return new byte[0];
            }

            return (_initializationVector == null) ? InternalEncryptionWithRandomIV(input) : InternalEncryption(input);
        }

        /// <inheritdoc cref="IEncryptionProvider.DecryptFromBase64(string)" />
        public virtual string? DecryptFromBase64(string? input) {
            if (string.IsNullOrEmpty(input)) return null;
            var decryptedData = Decrypt(Convert.FromBase64String(input));
            return Encoding.UTF8.GetString(decryptedData).Trim('\0');
        }

        /// <inheritdoc cref="IEncryptionProvider.Decrypt(byte[])" />
        public virtual byte[] Decrypt(byte[] input) {
            if (input is null || input.Length == 0) {
                return new byte[0];
            }

            return (_initializationVector == null) ? InternalDecryptionWithExtractedIV(input) : InternalDecryption(input, 0, input.Length);
        }

        /// <summary>
        ///     Dispose the internal resources for the operarion
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Dispose the internal resources for the operarion
        /// </summary>
        protected virtual void Dispose(bool disposing) {
            if (_aes != null) { _aes.Dispose(); }
        }

        /// <summary>
        ///     Generates an AES cryptography provider.
        /// </summary>
        /// <param name="key">AES key used for the symmetric encryption.</param>
        /// <param name="iv">AES Initialization Vector used for the symmetric encryption.</param>
        /// <param name="mode">Mode for operation used in the symmetric encryption.</param>
        /// <param name="padding">Padding mode used in the symmetric encryption.</param>
        /// <param name="blockSize">block size in bits</param>
        private Aes CreateCryptographyProvider(byte[] key, byte[]? iv, CipherMode mode, PaddingMode padding, int blockSize) {
            var aes = Aes.Create();

            aes.Mode = mode;
            aes.KeySize = key.Length * 8;
            aes.BlockSize = blockSize;
            aes.FeedbackSize = blockSize;
            aes.Padding = padding;
            aes.Key = key;
            if (iv != null) aes.IV = iv;

            return aes;
        }

        /// <summary>
        ///     Encrypt the passed input using a random-generated iv and return the iv joined with the encrypted data.
        /// </summary>
        private byte[] InternalEncryptionWithRandomIV(byte[] input) {
            byte[] iv;
            byte[] encryptedBytes;
            lock (_mySynchObject) {
                _aes.GenerateIV();
                iv = _aes.IV;
                encryptedBytes = InternalEncryption(input);
            }
            // Add the initialization vector
            var result = new byte[iv.Length + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
            return result;
        }

        /// <summary>
        ///     Encrypt the passed input.
        /// </summary>
        private byte[] InternalEncryption(byte[] input) {
            using (ICryptoTransform transform = _aes.CreateEncryptor()) {
                return transform.TransformFinalBlock(input, 0, input.Length);
                //// following lines are a longer version of the instruction above
                //// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                //using (MemoryStream memoryStream = new MemoryStream())
                //using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write)) {
                //    cryptoStream.Write(input, 0, input.Length);
                //    cryptoStream.FlushFinalBlock();
                //    memoryStream.Seek(0L, SeekOrigin.Begin);
                //    return StreamToBytes(memoryStream);
                //}
            }
        }

        /// <summary>
        ///     Decrypt the passed input using the IV extracted by the input itself.
        /// </summary>
        private byte[] InternalDecryptionWithExtractedIV(byte[] input) {
            // Extract the initialization vector
            var iv = new byte[_aes.IV.Length];
            Buffer.BlockCopy(input, 0, iv, 0, iv.Length);

            // decrypt and return it
            lock (_mySynchObject) {
                _aes.IV = iv;
                return InternalDecryption(input, iv.Length, input.Length - iv.Length);
            }
        }

        /// <summary>
        ///     Decrypt the passed input.
        /// </summary>
        private byte[] InternalDecryption(byte[] input, int inputOffset, int inputCount) {
            using (ICryptoTransform transform = _aes.CreateDecryptor()) {
                return transform.TransformFinalBlock(input, inputOffset,inputCount);
                //// following lines are a longer version of the instruction above
                //// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                //using (MemoryStream memoryStream = new MemoryStream(input))
                //using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read)) {
                //    return StreamToBytes(cryptoStream);
                //}
            }
        }

        /// <summary>
        ///     Converts a <see cref="Stream"/> into a byte array.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>The stream's content as a byte array.</returns>
        private byte[] StreamToBytes(Stream stream) {
            if (stream is MemoryStream ms) {
                return ms.ToArray();
            }

            using (var output = new MemoryStream()) {
                stream.CopyTo(output);
                return output.ToArray();
            }
        }

        /// <summary>
        ///     Generates an AES key.
        /// </summary>
        /// <remarks>
        ///     The key size of the Aes encryption must be 128, 192 or 256 bits. 
        ///     Please check https://blogs.msdn.microsoft.com/shawnfa/2006/10/09/the-differences-between-rijndael-and-aes/ for more information.
        /// </remarks>
        /// <param name="keySize">AES Key size</param>
        /// <returns></returns>
        public static (byte[] key, byte[] iv) GenerateKey(AesKeySize keySize) {
            using (var aes = Aes.Create()) {
                aes.KeySize = (int)keySize;
                aes.BlockSize = DefaultAesBlockSize;

                aes.GenerateKey();
                aes.GenerateIV();

                return (aes.Key, aes.IV);
            }
        }
    }
}
