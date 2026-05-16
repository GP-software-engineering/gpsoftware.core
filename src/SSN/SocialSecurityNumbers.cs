using System.Collections.Generic;
using System.Numerics;
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

        /// <summary>
        /// Validates an Italian Codice Fiscale (CF).
        /// Checks format, omocodia, and control character.
        /// </summary>
        public static bool IsValidCodiceFiscale(string cfInput) {
            if (string.IsNullOrWhiteSpace(cfInput)) {
                return false;
            }

            string cleanedCf = Regex.Replace(cfInput.ToUpper(), @"[\s.]", "");

            if (cleanedCf.Length != 16) {
                return false;
            }

            const string cfPattern = @"^[A-Z]{6}[A-Z0-9LMNPQRSTUV]{2}[ABCDEHLMPRST]{1}[A-Z0-9LMNPQRSTUV]{2}[A-Z]{1}[A-Z0-9LMNPQRSTUV]{3}[A-Z]{1}$";
            if (!Regex.IsMatch(cleanedCf, cfPattern)) {
                return false;
            }

            int sum = 0;
            string first15Chars = cleanedCf.Substring(0, 15);

            for (int i = 0; i < 15; i++) {
                char currentChar = first15Chars[i];
                int? charValue = null;

                if ((i + 1) % 2 != 0) // Posizioni dispari
                {
                    if (CfOddCharsMap.TryGetValue(currentChar, out int oddVal)) {
                        charValue = oddVal;
                    }
                } else // Posizioni pari
                  {
                    if (currentChar >= '0' && currentChar <= '9') {
                        charValue = int.Parse(currentChar.ToString());
                    } else if (currentChar >= 'A' && currentChar <= 'Z') {
                        charValue = currentChar - 'A';
                    }
                }

                if (charValue == null) {
                    return false;
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
        public static bool IsValidSwissAVS(string avsInput) {
            if (string.IsNullOrWhiteSpace(avsInput)) {
                return false;
            }

            string cleanedAvs = Regex.Replace(avsInput.ToUpper(), @"[\s.]", "");

            if (!Regex.IsMatch(cleanedAvs, @"^\d{13}$") || !cleanedAvs.StartsWith("756")) {
                return false;
            }

            if (!int.TryParse(cleanedAvs[12].ToString(), out int actualCheckDigit)) {
                return false;
            }

            int sum = 0;
            for (int i = 0; i < 12; i++) {
                if (!int.TryParse(cleanedAvs[i].ToString(), out int digit)) {
                    return false;
                }
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
        public static bool IsValidAustrianSVNR(string svnrInput) {
            if (string.IsNullOrWhiteSpace(svnrInput)) {
                return false;
            }

            string cleanedSvnr = Regex.Replace(svnrInput.ToUpper(), @"[-\/\s]", "");

            if (!Regex.IsMatch(cleanedSvnr, @"^\d{10}$")) {
                return false;
            }

            int[] digits = new int[10];
            for (int i = 0; i < 10; i++) {
                if (!int.TryParse(cleanedSvnr[i].ToString(), out digits[i])) {
                    return false;
                }
            }

            int sum =
                digits[0] * 3 +
                digits[1] * 7 +
                digits[2] * 9 +
                // digits[3] is the check digit
                digits[4] * 5 +
                digits[5] * 4 +
                digits[6] * 8 +
                digits[7] * 6 +
                digits[8] * 1 +
                digits[9] * 2;

            int remainder = sum % 11;

            // In Austria, if the remainder is 10, the SSN is strictly invalid and cannot exist.
            if (remainder == 10) {
                return false;
            }

            return remainder == digits[3];
        }

        #endregion

        #region French INSEE Number

        /// <summary>
        /// Validates a French Social Security Number (INSEE number - NIR).
        /// Handles Corsica alphanumeric departments (2A, 2B) and uses BigInteger for modulo.
        /// </summary>
        public static bool IsValidFrenchINSEE(string inseeInput) {
            if (string.IsNullOrWhiteSpace(inseeInput)) {
                return false;
            }

            string cleanedInsee = Regex.Replace(inseeInput.ToUpper(), @"[\s.]", "");

            // Regex updated: 6 digits, then 1 char (0-9, A or B for Corsica), then 8 digits. Total 15.
            if (!Regex.IsMatch(cleanedInsee, @"^\d{6}[0-9AB]\d{8}$")) {
                return false;
            }

            char sexDigit = cleanedInsee[0];
            if (sexDigit != '1' && sexDigit != '2' && sexDigit != '3' && sexDigit != '4' && sexDigit != '7' && sexDigit != '8') {
                return false;
            }

            string numberPartStr = cleanedInsee.Substring(0, 13);
            string keyPartStr = cleanedInsee.Substring(13, 2);

            if (!int.TryParse(keyPartStr, out int keyPartInt)) {
                return false;
            }

            // Replace Corsica departments with their mathematical equivalents for the Modulo 97 calculation
            string mathCalculationStr = numberPartStr.Replace("2A", "19").Replace("2B", "18");

            try {
                if (!BigInteger.TryParse(mathCalculationStr, out BigInteger numberBigInt)) {
                    return false;
                }

                BigInteger remainderBigInt = numberBigInt % new BigInteger(97);
                int remainder = (int)remainderBigInt;

                int calculatedKey = 97 - remainder;

                return calculatedKey == keyPartInt;
            }
            catch {
                return false;
            }
        }

        #endregion

        #region Spanish NIF / NIE (DNI)

        /// <summary>
        /// Validates a Spanish NIF (DNI for citizens) or NIE (for foreigners).
        /// Checks the format and validates the Modulo 23 control character.
        /// Does not validate CIF (corporate IDs).
        /// </summary>
        public static bool IsValidSpanishNIF(string nifInput) {
            if (string.IsNullOrWhiteSpace(nifInput)) {
                return false;
            }

            string cleanedNif = Regex.Replace(nifInput.ToUpper(), @"[\s\-]", "");

            // Matches DNI (8 digits + 1 letter) or NIE (X, Y, Z + 7 digits + 1 letter)
            if (!Regex.IsMatch(cleanedNif, @"^[XYZ0-9]\d{7}[A-Z]$")) {
                return false;
            }

            string numberStr = cleanedNif.Substring(0, 8);

            // Transform foreign resident NIEs to standard numbers to calculate the checksum
            numberStr = numberStr.Replace('X', '0').Replace('Y', '1').Replace('Z', '2');

            if (!int.TryParse(numberStr, out int number)) {
                return false;
            }

            // String of control letters defined by the Spanish government algorithm
            const string controlLetters = "TRWAGMYFPDXBNJZSQVHLCKE";
            char expectedLetter = controlLetters[number % 23];
            char actualLetter = cleanedNif[8];

            return actualLetter == expectedLetter;
        }

        #endregion

        #region UK National Insurance Number (NINO)

        /// <summary>
        /// Validates a UK National Insurance Number (NINO).
        /// Checks specific prefix and format restrictions. 
        /// Note: The UK NINO relies entirely on structural regex rules and has no mathematical checksum.
        /// </summary>
        public static bool IsValidUKNINO(string ninoInput) {
            if (string.IsNullOrWhiteSpace(ninoInput)) {
                return false;
            }

            string cleanedNino = Regex.Replace(ninoInput.ToUpper(), @"[\s\-]", "");

            // UK NINO rules:
            // 2 letters, 6 digits, 1 letter (A, B, C, or D).
            // Letters D, F, I, Q, U, V are not used in prefixes. O is additionally skipped in the 2nd letter.
            // Prefixes BG, GB, NK, KN, TN, NT, ZZ are not allocated by the government.
            const string ninoPattern = @"^(?!BG|GB|NK|KN|TN|NT|ZZ)[A-CEGHJ-PR-TW-Z][A-CEGHJ-NPR-TW-Z]\d{6}[A-D]$";

            return Regex.IsMatch(cleanedNino, ninoPattern);
        }

        #endregion

        #region Portuguese NIF

        /// <summary>
        /// Validates a Portuguese NIF (Número de Identificação Fiscal).
        /// Identifies formats for both natural persons and entities and validates the Modulo 11 check digit.
        /// </summary>
        public static bool IsValidPortugueseNIF(string nifInput) {
            if (string.IsNullOrWhiteSpace(nifInput)) {
                return false;
            }

            string cleanedNif = Regex.Replace(nifInput.ToUpper(), @"[\s\-]", "");

            // People often type the international ISO country code in front (e.g. PT 501 964 843)
            if (cleanedNif.StartsWith("PT")) {
                cleanedNif = cleanedNif.Substring(2);
            }

            if (!Regex.IsMatch(cleanedNif, @"^\d{9}$")) {
                return false;
            }

            int[] digits = new int[9];
            for (int i = 0; i < 9; i++) {
                digits[i] = cleanedNif[i] - '0';
            }

            // Calculation based on Modulo 11
            int sum = 0;
            for (int i = 0; i < 8; i++) {
                sum += digits[i] * (9 - i);
            }

            int expectedCheckDigit = 11 - (sum % 11);

            // Standard Portuguese "quirk" in the algorithm: if the result is 10 or 11, it falls back to 0
            if (expectedCheckDigit >= 10) {
                expectedCheckDigit = 0;
            }

            return expectedCheckDigit == digits[8];
        }

        #endregion
    }
}
