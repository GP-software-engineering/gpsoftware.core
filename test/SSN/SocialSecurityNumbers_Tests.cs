using GPSoftware.Core.SSN;
using GPSoftware.Core.Validation;
using Shouldly;
using Xunit;

namespace GPSoftware.Core.Tests.SSN {

    public class SocialSecurityNumbers_Tests {

        // =======================================================================
        // ITALIAN CODICE FISCALE
        // =======================================================================

        [Fact]
        public void IsValidCodiceFiscale_ReturnsTrue_OnValidFormat() {
            // Valid CF (Formal check)
            SocialSecurityNumbers.IsValidCodiceFiscale("RSSMRA80A01H501U").ShouldBeTrue();
            // Valid CF with spaces and lowercase
            SocialSecurityNumbers.IsValidCodiceFiscale("rss mra 80A01 h501u").ShouldBeTrue();
        }

        [Fact]
        public void IsValidCodiceFiscale_ReturnsFalse_OnInvalidChecksum() {
            // Changed last character from U to Z
            SocialSecurityNumbers.IsValidCodiceFiscale("RSSMRA80A01H501Z").ShouldBeFalse();
        }

        // =======================================================================
        // SWISS AVS
        // =======================================================================

        [Fact]
        public void IsValidSwissAVS_ReturnsTrue_OnValidFormat() {
            // Valid Swiss AVS mathematically generated
            SocialSecurityNumbers.IsValidSwissAVS("756.1234.5678.97").ShouldBeTrue();
        }

        [Fact]
        public void IsValidSwissAVS_ReturnsFalse_OnInvalidPrefix() {
            // Must start with 756
            SocialSecurityNumbers.IsValidSwissAVS("123.1234.5678.97").ShouldBeFalse();
        }

        // =======================================================================
        // AUSTRIAN SVNR
        // =======================================================================

        [Fact]
        public void IsValidAustrianSVNR_ReturnsTrue_OnValidFormat() {
            // Valid SVNR: Check digit is 7 for 123X010180 -> 1237010180
            SocialSecurityNumbers.IsValidAustrianSVNR("1237-010180").ShouldBeTrue();
        }

        [Fact]
        public void IsValidAustrianSVNR_ReturnsFalse_OnInvalidFormat() {
            SocialSecurityNumbers.IsValidAustrianSVNR("1238-010180").ShouldBeFalse();
        }

        // =======================================================================
        // FRENCH INSEE
        // =======================================================================

        [Fact]
        public void IsValidFrenchINSEE_ReturnsTrue_OnValidFormat() {
            // Valid French INSEE. 
            // Base: 1800189123045. 1800189123045 % 97 = 4. 97 - 4 = 93.
            SocialSecurityNumbers.IsValidFrenchINSEE("1 80 01 89 123 045 93").ShouldBeTrue();
        }

        [Fact]
        public void IsValidFrenchINSEE_ReturnsTrue_OnCorsicaFormat() {
            // Valid French INSEE with Corsica department (2A)
            // 2A becomes 19 -> Base: 1800119123045. 1800119123045 % 97 = 54. 97 - 54 = 43.
            SocialSecurityNumbers.IsValidFrenchINSEE("1 80 01 2A 123 045 43").ShouldBeTrue();
        }

        // =======================================================================
        // SPANISH NIF/NIE
        // =======================================================================

        [Fact]
        public void IsValidSpanishNIF_ReturnsTrue_OnValidDNI() {
            // 12345678 % 23 = 14 -> Z
            SocialSecurityNumbers.IsValidSpanishNIF("12345678Z").ShouldBeTrue();
        }

        [Fact]
        public void IsValidSpanishNIF_ReturnsTrue_OnValidNIE() {
            // X (0) 1234567 % 23 = 19 -> L
            SocialSecurityNumbers.IsValidSpanishNIF("X-1234567-L").ShouldBeTrue();
        }

        [Fact]
        public void IsValidSpanishNIF_ReturnsFalse_OnInvalidChecksum() {
            SocialSecurityNumbers.IsValidSpanishNIF("12345678A").ShouldBeFalse();
        }

        // =======================================================================
        // UK NINO
        // =======================================================================

        [Fact]
        public void IsValidUKNINO_ReturnsTrue_OnValidFormat() {
            // Standard valid format
            SocialSecurityNumbers.IsValidUKNINO("AB 12 34 56 C").ShouldBeTrue();
        }

        [Fact]
        public void IsValidUKNINO_ReturnsFalse_OnInvalidPrefix() {
            // BG is an explicitly forbidden prefix
            SocialSecurityNumbers.IsValidUKNINO("BG 12 34 56 C").ShouldBeFalse();
        }

        // =======================================================================
        // PORTUGUESE NIF
        // =======================================================================

        [Fact]
        public void IsValidPortugueseNIF_ReturnsTrue_OnValidFormat() {
            // PT NIF valid checksum: 123456789. Expected check digit is 9.
            SocialSecurityNumbers.IsValidPortugueseNIF("PT 123 456 789").ShouldBeTrue();
            SocialSecurityNumbers.IsValidPortugueseNIF("123456789").ShouldBeTrue();
        }

        [Fact]
        public void IsValidPortugueseNIF_ReturnsFalse_OnInvalidChecksum() {
            SocialSecurityNumbers.IsValidPortugueseNIF("123456780").ShouldBeFalse();
        }

        // =======================================================================
        // ATTRIBUTE TESTS
        // =======================================================================

        [Fact]
        public void Attribute_IsValid_ReturnsTrue_ForCorrectSingleType() {
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Italian);
            attribute.IsValid("RSSMRA80A01H501U").ShouldBeTrue();
        }

        [Fact]
        public void Attribute_IsValid_ReturnsFalse_ForWrongSingleType() {
            // It's a valid Spanish NIF, but the attribute expects an Italian CF
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Italian);
            attribute.IsValid("12345678Z").ShouldBeFalse();
        }

        [Fact]
        public void Attribute_IsValid_ReturnsTrue_ForCombinedTypes() {
            var attribute = new SocialSecurityNumberAttribute(
                SocialSecurityNumberAttribute.Types.Italian | SocialSecurityNumberAttribute.Types.Spanish);

            attribute.IsValid("RSSMRA80A01H501U").ShouldBeTrue(); // Italian passes
            attribute.IsValid("12345678Z").ShouldBeTrue();        // Spanish passes
            attribute.IsValid("AB123456C").ShouldBeFalse();       // UK fails
        }

        [Fact]
        public void Attribute_IsValid_ReturnsTrue_ForAnyType() {
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Any);

            attribute.IsValid("12345678Z").ShouldBeTrue(); // Spanish
            attribute.IsValid("AB123456C").ShouldBeTrue(); // UK
            attribute.IsValid("INVALID_SSN").ShouldBeFalse();
        }

        [Fact]
        public void Attribute_IsValid_HandlesNullAndEmpty_ReturnsTrue() {
            var attribute = new SocialSecurityNumberAttribute(SocialSecurityNumberAttribute.Types.Italian);

            attribute.IsValid(null).ShouldBeTrue();
            attribute.IsValid(string.Empty).ShouldBeTrue();
        }
    }
}
