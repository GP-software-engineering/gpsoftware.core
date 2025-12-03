using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GPSoftware.Core.Helpers {

    public static class UrlHelper {

        /// <summary>
        ///     Trim, replace blanks and a remove reserved chars from the passed string
        /// </summary>
        public static string NormalizeAsUrl(string slugToNorm) {
            slugToNorm = Regex.Replace(slugToNorm.Trim().ToLowerInvariant(), "[ _]{1,}", "-");
            slugToNorm = RemoveDiacritics(slugToNorm);
            slugToNorm = Regex.Replace(slugToNorm, "[^0-9a-zA-Z\\-]+", "");
            //slugToNorm = RemoveReservedUrlCharacters(slugToNorm);
            return slugToNorm;
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
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(string text) {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString) {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark) {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        ///     Remove characters not allowed in file names
        /// </summary>
        public static string RemoveInvalidFileNameChars(string input) {
            // ToDo: what we are doing here if we switch the blog from windows
            // to unix system or vice versa? we should remove all invalid chars for both systems

            var regexSearch = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars()));
            var r = new Regex($"[{regexSearch}]");
            return r.Replace(input, "");
        }
    }
}
    