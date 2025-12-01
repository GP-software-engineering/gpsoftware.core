using System;
using GPSoftware.Core.Extensions;

namespace GPSoftware.core.Tests.Extensions {

    public class StringExtension_Tests {

        public StringExtension_Tests() {
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("short")]
        [InlineData("medium string")]
        [InlineData("this is a longer test string")]
        public void SubstringOrEmpty_Test(string input) {
            // run and assert
            input.SubstringOrEmpty(0).ShouldBe(input);
            input.SubstringOrEmpty(input.Length + 1).ShouldBeNullOrEmpty();
            input.SubstringOrEmpty(0, input.Length).ShouldBe(input);
            input.SubstringOrEmpty(input.Length, 0).ShouldBeNullOrEmpty();
            input.SubstringOrEmpty(input.Length, 2).ShouldBeNullOrEmpty();
            input.SubstringOrEmpty(input.Length, input.Length).ShouldBeNullOrEmpty();
            input.SubstringOrEmpty(input.Length, input.Length + 1).ShouldBeNullOrEmpty();

            var output = input.SubstringOrEmpty(0, Math.Max(0, input.Length - 1));
            input.ShouldStartWith(output);

            output = input.SubstringOrEmpty(Math.Max(0, input.Length - 4), Math.Max(0, input.Length - 1));
            input.Contains(output).ShouldBeTrue();
        }


        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("short")]
        [InlineData("medium string")]
        [InlineData("this is a longer test string")]
        public void SubstringWithAppendix(string input) {
            // prepare
            string appendix = "...";

            // run and assert
            input.SubstringWithAppendix(0, input.Length, appendix).ShouldBe(input);

            if (string.IsNullOrEmpty(input)) return;

            // run and assert
            input.SubstringWithAppendix(input.Length + 1, input.Length, appendix).ShouldBe(appendix);

            var output = input.SubstringWithAppendix(0, Math.Max(0, input.Length - 1), appendix);
            output.Length.ShouldBeLessThanOrEqualTo(input.Length);
            output.EndsWith(appendix).ShouldBeTrue();

            output = input.SubstringWithAppendix(0, Math.Max(0, input.Length - 4), appendix);
            output.Length.ShouldBeLessThanOrEqualTo(input.Length);
            output.EndsWith(appendix).ShouldBeTrue();

            output = input.SubstringWithAppendix(3, 5, appendix);
            output.Length.ShouldBeLessThanOrEqualTo(input.Length);
        }

        [Theory]
        [InlineData(0, 10, "", "")]
        [InlineData(1, 10, "", "")]
        [InlineData(10, 0, "", "")]
        [InlineData(10, 10, "", "")]
        [InlineData(0, 2, "abcdef", "ab...")]
        [InlineData(0, 3, "abcdef", "abc...")]
        [InlineData(0, 4, "abcdef", "abc...")]
        [InlineData(0, 5, "abcdef", "abc...")]
        [InlineData(0, 6, "abcdef", "abcdef")]
        [InlineData(0, 10, "abcdef", "abcdef")]

        [InlineData(1, 2, "abcdef", "bc...")]
        [InlineData(1, 3, "abcdef", "bcd...")]
        [InlineData(1, 4, "abcdef", "bcd...")]
        [InlineData(1, 5, "abcdef", "bcdef")]
        [InlineData(1, 6, "abcdef", "bcdef")]
        [InlineData(2, 3, "abcdef", "cde...")]
        [InlineData(3, 4, "abcdef", "def")]
        [InlineData(4, 4, "abcdef", "ef")]
        [InlineData(10, 6, "abcdef", "...")]
        [InlineData(10, 10, "abcdef", "...")]
        public void SubstringWithAppendix_StartIndex_and_MaxLen(int startIndex, int maxLen, string input, string expectedResult) {
            // prepare
            const string appendix = "...";

            // run and assert
            var output = input.SubstringWithAppendix(startIndex, maxLen, appendix);
            output.ShouldBe(expectedResult);
            output.Length.ShouldBeLessThanOrEqualTo(input.Length);
        }

        [Fact]
        public void EnsureStartsWith_Test() {
            // prepare
            string input = "www.test.com";

            // run and assert
            input.EnsureStartsWith("www.").ShouldBe(input);

            input.EnsureStartsWith("WwW.").ShouldNotBe(input);
            input.EnsureStartsWith("WwW.").ShouldBe("WwW." + input);

            input.EnsureStartsWith("WwW.", StringComparison.InvariantCultureIgnoreCase).ShouldBe(input);
        }


        [Fact]
        public void EnsureEndsWith_Test() {
            // prepare
            string input = "www.test.com";

            // run and assert
            input.EnsureEndsWith(".com").ShouldBe(input);

            input.EnsureEndsWith(".coM").ShouldNotBe(input);
            input.EnsureEndsWith(".coM").ShouldBe(input + ".coM");

            input.EnsureEndsWith(".coM", StringComparison.InvariantCultureIgnoreCase).ShouldBe(input);
        }

    }
}
