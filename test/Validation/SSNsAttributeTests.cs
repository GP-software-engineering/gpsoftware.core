using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace GPSoftware.core.Tests.Validation {
    public class SSNsAttributeTests {
        public SSNsAttributeTests() {
        }

        [Theory]
        [InlineData(12345)]
        public void IsValid_NonStringInpute(object inputValue) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Italian);

            // Act
            bool isValid = attribute.IsValid(inputValue); // Esempio di input non stringa

            // Assert
            Assert.False(isValid);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData(" ", false)]
        [InlineData("CRTGPR68H22F839B", true)]
        [InlineData("CRT.GPR.68H.22F.839B", true)]
        [InlineData("crtgpr68H22F839b", true)]
        [InlineData("crtgpr.68H22 F839b", true)]
        [InlineData("RSS MR A80 A01 H501 U", true)]
        [InlineData("RSS.MRA.80A.01.H501U", true)]
        [InlineData("RSS.MRA.80A.01.H501Z", false)]
        [InlineData("190123306030016", false)]
        [InlineData("CRTGPR68Z22F839K", false)]
        [InlineData("INVALID_CF", false)]
        public void Italian_Validate(string testValue, bool mustBeValid) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Italian);

            // Act
            bool isValid = attribute.IsValid(testValue);

            // Assert
            Assert.Equal(mustBeValid, isValid);
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(" ", false)]
        [InlineData("7561234567897", true)]
        [InlineData("756.0000.0000.02", true)]
        [InlineData("756.1234.5678.92", false)]
        [InlineData("CRTGPR68H22F839B", false)]
        public void Swiss_Validate(string testValue, bool mustBeValid) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Swiss);

            // Act
            bool isValid = attribute.IsValid(testValue);

            // Assert
            Assert.Equal(mustBeValid, isValid);
        }

        [Theory]
        [InlineData("CRTGP R6 8H 22F83 9B", false)]
        [InlineData("CRTGPR68Z22F839K", false)]
        [InlineData("INVALID_CF", false)]
        public void Austrian_Validate(string testValue, bool mustBeValid) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Austrian);

            // Act
            bool isValid = attribute.IsValid(testValue);

            // Assert
            Assert.Equal(mustBeValid, isValid);
        }

        [Theory]
        [InlineData("190123306030016", true)]
        [InlineData("190 1233 060 300 16", true)]
        [InlineData("190123306030011", false)]
        [InlineData("116102603801457", false)]
        public void French_Validate(string testValue, bool mustBeValid) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.French);

            // Act
            bool isValid = attribute.IsValid(testValue);

            // Assert
            Assert.Equal(mustBeValid, isValid);
        }

        [Theory]
        [InlineData("CRTGPR68H22F839B", true)]
        [InlineData("CRTGPR68Z22F839K", false)]
        [InlineData("INVALID_CF", false)]
        public void ItalianOrSwiss_Validate(string testValue, bool mustBeValid) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Italian | SocialSecurityNumberAttribute.Types.Swiss);

            // Act
            bool isValid = attribute.IsValid(testValue);

            // Assert
            Assert.Equal(mustBeValid, isValid);
        }

        [Theory]
        [InlineData("CRTGPR68H22F839B", true)]
        [InlineData("190123306030016", true)]
        [InlineData("190.123.3060300.16", true)]
        [InlineData("7561234567897", true)]
        [InlineData("756.0000.0000.02", true)]
        [InlineData("RSS.MRA.80A.01.H501U", true)]
        [InlineData("RSS.MRA.80A.01.H501Z", false)]
        [InlineData("INVALID_CF", false)]
        public void AllSupportedFormats_Validate(string testValue, bool mustBeValid) {
            // Arrange
            var attribute = new SocialSecurityNumberAttribute(
                SocialSecurityNumberAttribute.Types.Italian
                | SocialSecurityNumberAttribute.Types.Swiss
                | SocialSecurityNumberAttribute.Types.French
                | SocialSecurityNumberAttribute.Types.Austrian
                );

            // Act
            bool isValid = attribute.IsValid(testValue);

            // Assert
            Assert.Equal(mustBeValid, isValid);
        }

    }

}