using System.Text.RegularExpressions;
using GPSoftware.Core.Crypto;


namespace GPSoftware.core.Tests.Crypto {

    public class Hasher_Tests {

        public Hasher_Tests() {
        }

        [Fact]
        public void ComputeSha1Hash_Test() {
            // prepare
            string input = "this is a test message to hash";

            // run
            string output = Hasher.ComputeSha1Hash(input);

            // assert
            output.ShouldNotBeNullOrWhiteSpace();
            output.Length.ShouldBe(40);
            Regex.IsMatch(output, "^[0-9a-fA-F]{40}$", RegexOptions.Compiled).ShouldBeTrue();
        }

        [Fact]
        public void ComputeSha256Hash_Test() {
            // prepare
            string input = "this is a test message to hash";

            // run
            string output = Hasher.ComputeSha256Hash(input);

            // assert
            output.ShouldNotBeNullOrEmpty();
            output.Length.ShouldBe(64);
            Regex.IsMatch(output, "^[0-9a-fA-F]{64}$", RegexOptions.Compiled).ShouldBeTrue();
        }
    }
}
