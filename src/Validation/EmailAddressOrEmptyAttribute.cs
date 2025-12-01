using System.Text.RegularExpressions;

namespace System.ComponentModel.DataAnnotations {

    /// <summary>
    ///     Extends the EmailAddressAttribute by also accepting empty email addresses ("").
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class EmailAddressOrEmptyAttribute : ValidationAttribute {

        // Standard regex HTML5
        private static readonly Regex _emailRegex = new Regex(
            @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool IsValid(object? value) {
            if (value == null || (value is string str && string.IsNullOrEmpty(str))) {
                return true;
            }

            string? input = value as string;
            if (string.IsNullOrEmpty(input)) return true;

            try {
                return _emailRegex.IsMatch(input!);
            } catch (RegexMatchTimeoutException) {
                // If regex times out, treat as invalid for safety
                return false;
            }
        }
    }
}