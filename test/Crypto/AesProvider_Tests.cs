using System.Text;
using GPSoftware.Core.Crypto;

namespace GPSoftware.Core.Tests.Crypto {

    public class AesProvider_Tests {

        const string _input = "this is a test message of 1000€ to encypt and decrypt Using \n\t etc.!";

        public AesProvider_Tests() {
        }

        [Fact]
        public void Encrypt_empty_data() {
            // prepare
            string plainKey = "This is my k€!";
            using var enc = new AesProvider(plainKey);

            // run
            byte[] outputBin = enc.Encrypt(new byte[0]);
            outputBin.ShouldNotBeNull();
            outputBin.Length.ShouldBe(0);

            // run
            outputBin = enc.Encrypt((string?)null);
            outputBin.ShouldNotBeNull();
            outputBin.Length.ShouldBe(0);

            // run
            outputBin = enc.Encrypt(string.Empty);
            outputBin.ShouldNotBeNull();
            outputBin.Length.ShouldBe(0);

            // run
            string? outputStr = enc.EncryptToBase64((string?)null);
            outputStr.ShouldBeNull();

            // run
            outputStr = enc.EncryptToBase64(string.Empty);
            outputStr.ShouldBeNull();
        }

        [Fact]
        public void Decrypt_empty_data() {
            // prepare
            string plainKey = "This is my k€!";
            using var enc = new AesProvider(plainKey);

            // run
            byte[] outputBin = enc.Decrypt(new byte[0]);
            outputBin.ShouldNotBeNull();
            outputBin.Length.ShouldBe(0);

            // run
            string? outputStr = enc.DecryptFromBase64(null);
            outputStr.ShouldBeNull();

            // run
            outputStr = enc.DecryptFromBase64(string.Empty);
            outputStr.ShouldBeNull();
        }

        [Fact]
        public void Encrypt_array_with_fixed_key_and_iv() {
            // prepare
            var key = AesProvider.GenerateKey(AesKeySize.AES192Bits);
            using var enc1 = new AesProvider(key.key, key.iv);
            using var enc2 = new AesProvider(key.key, key.iv);

            // run
            byte[] output1 = enc1.Encrypt(Encoding.UTF8.GetBytes(_input));
            byte[] output2 = enc1.Encrypt(Encoding.UTF8.GetBytes(_input));
            byte[] output3 = enc2.Encrypt(Encoding.UTF8.GetBytes(_input));

            // assert
            output1.ShouldNotBe(Encoding.UTF8.GetBytes(_input));
            output2.ShouldNotBe(Encoding.UTF8.GetBytes(_input));
            output3.ShouldNotBe(Encoding.UTF8.GetBytes(_input));
            output1.SequenceEqual(output2).ShouldBeTrue();
            output2.SequenceEqual(output3).ShouldBeTrue();
            output1.ShouldBe(output2);
            output2.ShouldBe(output3);

            // ================================================
            // run 
            output1 = enc1.Decrypt(output1);
            output2 = enc2.Decrypt(output2);
            output3 = enc2.Decrypt(output3);

            // assert
            Encoding.UTF8.GetString(output1).ShouldBe(_input);
            Encoding.UTF8.GetString(output2).ShouldBe(_input);
            Encoding.UTF8.GetString(output3).ShouldBe(_input);
        }

        [Fact]
        public void Encrypt_array_with_plain_key_and_random_IV() {
            // prepare
            string plainKey = "This is my k€!";
            using var enc1 = new AesProvider(plainKey);
            using var enc2 = new AesProvider(plainKey);

            // run
            byte[] output1a = enc1.Encrypt(Encoding.UTF8.GetBytes(_input));
            byte[] output1b = enc1.Encrypt(Encoding.UTF8.GetBytes(_input));

            byte[] output2a = enc2.Encrypt(Encoding.UTF8.GetBytes(_input));
            byte[] output2b = enc2.Encrypt(Encoding.UTF8.GetBytes(_input));

            // assert
            output1a.SequenceEqual(output1b).ShouldBeFalse();
            output1a.SequenceEqual(output2a).ShouldBeFalse();
            output2a.SequenceEqual(output2b).ShouldBeFalse();

            // ================================================
            // run 
            output1a = enc1.Decrypt(output1a);
            output1b = enc1.Decrypt(output1b);
            output2a = enc2.Decrypt(output2a);
            output2b = enc2.Decrypt(output2b);

            // assert
            Encoding.UTF8.GetString(output1a).ShouldBe(_input);
            Encoding.UTF8.GetString(output1b).ShouldBe(_input);
            Encoding.UTF8.GetString(output2a).ShouldBe(_input);
            Encoding.UTF8.GetString(output2b).ShouldBe(_input);
        }

        [Fact]
        public void Encrypt_string_with_random_key() {
            // prepare
            var key = AesProvider.GenerateKey(AesKeySize.AES192Bits);
            using var enc1 = new AesProvider(key.key, key.iv);
            using var enc2 = new AesProvider(key.key, key.iv);

            // run
            string? output1 = enc1.EncryptToBase64(_input);
            string? output2 = enc2.EncryptToBase64(_input);

            // assert
            output1.ShouldNotBeNullOrEmpty();
            output1.ShouldBe(output2);

            // ================================================
            // run 
            output1 = enc1.DecryptFromBase64(output1);
            output2 = enc2.DecryptFromBase64(output2);

            // assert
            output1.ShouldBe(_input);
            output2.ShouldBe(_input);
        }

        [Fact]
        public void Encrypt_array_with_plain_key_and_plain_Salt() {
            // prepare
            string plainKey = "This is my k€!";
            string salt = "This is my k€!";
            
            using var enc1 = new AesProvider(plainKey, salt);
            using var enc2 = new AesProvider(plainKey, salt);

            // run
            var output1a = enc1.EncryptToBase64(_input);
            var output1b = enc1.EncryptToBase64(_input);
            var output2a = enc2.EncryptToBase64(_input);
            var output2b = enc2.EncryptToBase64(_input);

            // assert
            output1a.ShouldBe(output1b); // Same Salt = Same IV = Deterministic
            output1a.ShouldBe(output2a);
            output1a.ShouldBe(output2b);

            // ================================================
            // run 
            var plain1a = enc1.DecryptFromBase64(output1a);
            var plain1b = enc1.DecryptFromBase64(output1b);
            var plain2a = enc2.DecryptFromBase64(output2a);
            var plain2b = enc2.DecryptFromBase64(output2b);

            // assert
            plain1a.ShouldBe(_input);
            plain1b.ShouldBe(_input);
            plain2a.ShouldBe(_input);
            plain2b.ShouldBe(_input);
        }
    }
}
