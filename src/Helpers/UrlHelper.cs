using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GPSoftware.Core.Helpers {

    public static class UrlHelper {

        // Use a HashSet for O(1) lookup performance.
        private static readonly HashSet<char> _invalidChars;

        // Compiled Regex for performance

        // Matches anything that is NOT alphanumeric or a hyphen.
        private static readonly Regex SlugAllowListRegex = new Regex(@"[^a-z0-9\-]", RegexOptions.Compiled);
        // Matches multiple consecutive hyphens.
        private static readonly Regex MultipleHyphensRegex = new Regex(@"\-{2,}", RegexOptions.Compiled);

        static UrlHelper() {
            _invalidChars = new HashSet<char>();

            // 1. Add invalid characters from the current OS
            _invalidChars.UnionWith(Path.GetInvalidFileNameChars());
            _invalidChars.UnionWith(Path.GetInvalidPathChars());

            // 2. Enforce Windows restrictions even if running on Linux/Mac.
            // This ensures the filenames are valid if transferred to a Windows system later.
            // Windows forbidden chars: < > : " / \ | ? *
            var windowsSpecificInvalid = new[] { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            _invalidChars.UnionWith(windowsSpecificInvalid);
        }

        /// <summary>
        ///     Generates a SEO-friendly URL slug from a string.
        ///     Example: "Ciao Mondo! È un bel giorno" -> "ciao-mondo-e-un-bel-giorno"
        /// </summary>
        /// <param name="slugToNorm">The raw string input.</param>
        /// <returns>A normalized, lowercase, hyphen-separated string.</returns>
        public static string NormalizeAsUrl(string? slugToNorm) {
            if (string.IsNullOrWhiteSpace(slugToNorm)) return string.Empty;

            // 1. Remove diacritics (accents) first
            slugToNorm = RemoveDiacritics(slugToNorm!);

            // 2. Convert to lowercase and trim whitespace
            slugToNorm = slugToNorm.ToLowerInvariant().Trim();

            // 3. Replace spaces and underscores with hyphens
            slugToNorm = slugToNorm.Replace(' ', '-').Replace('_', '-');

            // 4. Remove all characters that are not a-z, 0-9 or hyphen
            slugToNorm = SlugAllowListRegex.Replace(slugToNorm, "");

            // 5. Collapse multiple hyphens into one (e.g. "hello--world" -> "hello-world")
            slugToNorm = MultipleHyphensRegex.Replace(slugToNorm, "-");

            // 6. Trim hyphens from start and end (e.g. "-hello-" -> "hello")
            return slugToNorm.Trim('-');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveReservedUrlCharacters(string text) {
            var reservedCharacters = new List<string> { "!", "#", "$", "&", "'", "(", ")", "*", ",", "/", ":", ";", "=", "?", "@", "[", "]", "\"", "%", ".", "<", ">", "\\", "^", "_", "'", "{", "}", "|", "~", "`", "+" };
            foreach (var chr in reservedCharacters) text = text.Replace(chr, "");
            return text;
        }

        /// <summary>
        ///     Removes diacritics (accents) from a string.
        ///     Example: "È" -> "E", "ç" -> "c"
        /// </summary>
        /// <param name="text">The text to normalize.</param>
        /// <returns>ASCII-compatible string.</returns>
        public static string RemoveDiacritics(string text) {
            if (string.IsNullOrEmpty(text)) return text;

            var normalizedString = text!.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(text.Length);
            foreach (var c in normalizedString) {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Removes characters not allowed in file names.
        /// Ensures compatibility across both Windows and Unix systems.
        /// </summary>
        public static string? RemoveInvalidFileNameChars(string? input) {
            if (string.IsNullOrEmpty(input)) return input;

            // Quick pass to check if we actually need to do anything.
            int invalidCount = 0;
            foreach (char c in input) {
                if (IsInvalid(c)) invalidCount++;
            }

            // If the string is clean, return the original reference (Zero Allocation).
            if (invalidCount == 0) return input;

            var sb = new System.Text.StringBuilder(input.Length - invalidCount);
            foreach (char c in input) {
                if (!IsInvalid(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Helper to determine if a char is invalid.
        /// Checks the cached set and also control characters (like tab, newline, etc.).
        /// </summary>
        private static bool IsInvalid(char c) {
            return _invalidChars.Contains(c) || char.IsControl(c);
        }
    }
}
