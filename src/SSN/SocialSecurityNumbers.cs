using System;
using System.Collections.Generic;
using System.Numerics; // Required for BigInteger (for French INSEE)
using System.Text.RegularExpressions;

namespace GPSoftware.Core.SSN {

    /// <summary>
    ///     Provides validation methods for various national identification codes (SSNs).
    /// </summary>
    public static class SocialSecurityNumbers {

        #region Italian Codice Fiscale

        private static readonly Dictionary<char, int> CfOddCharsMap = new Dictionary<char, int> {
            {'0', 1}, {'1', 0}, {'2', 5}, {'3', 7}, {'4', 9}, {'5', 13}, {'6', 15}, {'7', 17}, {'8', 19}, {'9', 21},
            {'A', 1}, {'B', 0}, {'C', 5}, {'D', 7}, {'E', 9}, {'F', 13}, {'G', 15}, {'H', 17}, {'I', 19}, {'J', 21},
            {'K', 2}, {'L', 4}, {'M', 18}, {'N', 20}, {'O', 11}, {'P', 3}, {'Q', 6}, {'R', 8}, {'S', 12}, {'T', 14},
            {'U', 16}, {'V', 10}, {'W', 22}, {'X', 25}, {'Y', 24}, {'Z', 23}
        };

        private static readonly Dictionary<char, int> CfOmocodiaToDigitMap = new Dictionary<char, int> {
            {'L', 0}, {'M', 1}, {'N', 2}, {'P', 3}, {'Q', 4}, {'R', 5}, {'S', 6}, {'T', 7}, {'U', 8}, {'V', 9}
        };

        // Zero-indexed positions in the first 15 characters of an Italian CF that are normally numeric
        // and can contain omocodia replacement letters (L-V).
        // Corresponds to: Year (6,7), Day (9,10), PlaceCode Numerals (12,13,14).
        private static readonly HashSet<int> CfOmocodiaAffectedPositions = new HashSet<int> { 6, 7, 9, 10, 12, 13, 14 };

        /// <summary>
        /// Validates an Italian Codice Fiscale (CF).
        /// Checks format and control character. Does not validate date or place code against databases.
        /// </summary>
        /// <param name="cfInput">The Codice Fiscale string. Can contain spaces.</param>
        /// <returns>True if formally correct, otherwise false.</returns>
        public static bool IsValidCodiceFiscale(string cfInput) {
            if (string.IsNullOrWhiteSpace(cfInput)) {
                return false;
            }

            string cleanedCf = Regex.Replace(cfInput.ToUpper(), @"[\s.]", "");

            if (cleanedCf.Length != 16) {
                return false;
            }

            // Regex for CF structure: LLLLLL NN L NN L NNN C (L=Letter, N=NumericOrOmocodiaLetter for designated spots)
            // Corresponds to: Surname(3 L), Name(3 L), Year(2 N/Omo), Month(1 L), Day(2 N/Omo), Place(1L + 3 N/Omo), Control(1 L)
            // Adjusted for C# (no need to escape A-Z, 0-9 inside character groups as much)
            const string cfPattern = @"^[A-Z]{6}[A-Z0-9LMNPQRSTUV]{2}[ABCDEHLMPRST]{1}[A-Z0-9LMNPQRSTUV]{2}[A-Z]{1}[A-Z0-9LMNPQRSTUV]{3}[A-Z]{1}$";
            if (!Regex.IsMatch(cleanedCf, cfPattern)) {
                return false;
            }

            int sum = 0;
            string first15Chars = cleanedCf.Substring(0, 15);

            for (int i = 0; i < 15; i++) {
                char currentChar = first15Chars[i];
                char charForLookup = currentChar; // char to use for map lookups, may be converted from omocodia

                // Handle omocodia: if in a numeric-expected field and it's an omocodia letter,
                // convert to its digit char representation for map lookup
                if (CfOmocodiaAffectedPositions.Contains(i) && CfOmocodiaToDigitMap.TryGetValue(currentChar, out int digitEquivalent)) {
                    charForLookup = digitEquivalent.ToString()[0]; // Convert int digit to char '0'-'9'
                }

                int? charValue = null;

                if ((i + 1) % 2 != 0) // Odd positions (1st, 3rd, ..., 15th)
                {
                    if (CfOddCharsMap.TryGetValue(charForLookup, out int oddVal)) {
                        charValue = oddVal;
                    }
                } else // Even positions (2nd, 4th, ..., 14th)
                  {
                    if (charForLookup >= '0' && charForLookup <= '9') {
                        charValue = int.Parse(charForLookup.ToString());
                    } else if (charForLookup >= 'A' && charForLookup <= 'Z') // Letters A-Z
                      {
                        charValue = charForLookup - 'A';
                    }
                }

                if (charValue == null) {
                    return false; // Character not valid in its position for checksum
                }
                sum += charValue.Value;
            }

            int expectedControlCharValue = sum % 26;
            char expectedControlChar = (char)('A' + expectedControlCharValue);
            char actualControlChar = cleanedCf[15];

            return actualControlChar == expectedControlChar;
        }

        #endregion

        #region Swiss AVS Number

        /// <summary>
        /// Validates a Swiss AVS (AHV) number (new 13-digit format - AHVN13/NAVS13).
        /// Checks format, "756" prefix, and EAN-13 checksum.
        /// </summary>
        /// <param name="avsInput">The Swiss AVS number string. Can contain spaces or dots.</param>
        /// <returns>True if formally correct, otherwise false.</returns>
        public static bool IsValidSwissAVS(string avsInput) {
            if (string.IsNullOrWhiteSpace(avsInput)) {
                return false;
            }

            string cleanedAvs = Regex.Replace(avsInput.ToUpper(), @"[\s.]", "");

            if (!Regex.IsMatch(cleanedAvs, @"^\d{13}$") || !cleanedAvs.StartsWith("756")) {
                return false;
            }

            if (!int.TryParse(cleanedAvs[12].ToString(), out int actualCheckDigit)) {
                return false; // Should not happen if regex passed
            }

            int sum = 0;
            for (int i = 0; i < 12; i++) {
                if (!int.TryParse(cleanedAvs[i].ToString(), out int digit)) {
                    return false; // Should not happen
                }
                // Apply EAN-13 weights: 1 for odd positions (1st, 3rd,... which are index 0, 2,...), 3 for even positions
                sum += digit * ((i % 2 == 0) ? 1 : 3);
            }

            int calculatedCheckDigit = (10 - (sum % 10)) % 10;

            return calculatedCheckDigit == actualCheckDigit;
        }

        #endregion

        #region Austrian SVNR

        /// <summary>
        /// Validates an Austrian Social Insurance Number (SVNR).
        /// Supports SVNRs written as a plain 10-digit string or with common delimiters.
        /// </summary>
        /// <param name="svnrInput">The SVNR string. Can contain spaces, hyphens, or slashes.</param>
        /// <returns>True if formally correct, otherwise false.</returns>
        public static bool IsValidAustrianSVNR(string svnrInput) {
            if (string.IsNullOrWhiteSpace(svnrInput)) {
                return false;
            }

            string cleanedSvnr = Regex.Replace(svnrInput, @"[-\/\s]", "");

            if (!Regex.IsMatch(cleanedSvnr, @"^\d{10}$")) {
                return false;
            }

            int[] digits = new int[10];
            for (int i = 0; i < 10; i++) {
                if (!int.TryParse(cleanedSvnr[i].ToString(), out digits[i])) {
                    return false; // Should not happen
                }
            }

            int sum =
                digits[0] * 3 +
                digits[1] * 7 +
                digits[2] * 9 +
                // digits[3] is the actualCheckDigit (C), skipped in sum
                digits[4] * 5 +
                digits[5] * 4 +
                digits[6] * 8 +
                digits[7] * 6 +
                digits[8] * 1 +
                digits[9] * 2;

            int remainder = sum % 11;
            int calculatedCheckDigit = (remainder == 10) ? 0 : remainder;

            return calculatedCheckDigit == digits[3];
        }

        #endregion

        #region French INSEE Number

        /// <summary>
        /// Validates a French Social Security Number (INSEE number - NIR).
        /// Uses BigInteger for accurate modulo on the 13-digit number part.
        /// </summary>
        /// <param name="inseeInput">The INSEE number string. Can contain spaces.</param>
        /// <returns>True if formally correct, otherwise false.</returns>
        public static bool IsValidFrenchINSEE(string inseeInput) {
            if (string.IsNullOrWhiteSpace(inseeInput)) {
                return false;
            }

            string cleanedInsee = Regex.Replace(inseeInput.ToUpper(), @"[\s.]", "");

            if (!Regex.IsMatch(cleanedInsee, @"^\d{15}$")) {
                return false;
            }

            // Simplified semantic checks (can be expanded for full strictness)
            char sexDigit = cleanedInsee[0];
            if (sexDigit != '1' && sexDigit != '2') {
                // Allowing only common male (1) and female (2) for this validator.
                // Official specs include other digits for specific cases (3,4,7,8).
                // Consider if this check should return false or just be a warning.
            }

            if (!int.TryParse(cleanedInsee.Substring(3, 2), out int month) || (month < 1 || month > 12)) {
                // Allowing only standard months 01-12. Special codes (>20) exist.
                // Consider if this check should return false or just be a warning.
            }

            string numberPartStr = cleanedInsee.Substring(0, 13);
            string keyPartStr = cleanedInsee.Substring(13, 2); // Corrected from (13,15) to (13,2)

            if (!int.TryParse(keyPartStr, out int keyPartInt)) {
                return false; // Should not happen if regex passed
            }

            try {
                if (!BigInteger.TryParse(numberPartStr, out BigInteger numberBigInt)) {
                    return false; // Not a valid number string for BigInteger
                }

                BigInteger remainderBigInt = numberBigInt % new BigInteger(97);
                int remainder = (int)remainderBigInt; // Safe as remainder is 0-96

                int calculatedKey = 97 - remainder;
                // The French key can be 97 if remainder is 0. It is represented as 01-97.
                // The formula 97 - remainder works directly for this range.

                return calculatedKey == keyPartInt;
            } catch (Exception ex) // Catch potential errors from BigInteger or parsing
              {
                // Log error if necessary: Console.Error.WriteLine($"Error during INSEE validation: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
