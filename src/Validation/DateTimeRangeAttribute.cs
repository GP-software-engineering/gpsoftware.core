using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GPSoftware.Core.Validation {

    /// <summary>
    ///     Validates that a <see cref="DateTime"/> property falls within a dynamic range.
    ///     Supports specific ISO 8601 dates (e.g. "2025-01-23") or relative expressions like "Now", "Now+1d", "Now-1y".
    ///     <see cref="ValidationAttribute.ErrorMessageResourceType"/> and <see cref="ValidationAttribute.ErrorMessageResourceName"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RangeAttribute : ValidationAttribute {

        /// <summary>
        ///     The minimum allowed date string (e.g., "2023-01-01" or "Now-5d").
        /// </summary>
        public string Minimum { get; }

        /// <summary>
        ///     The maximum allowed date string (e.g., "2030-12-31" or "Now+1y").
        /// </summary>
        public string Maximum { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RangeAttribute"/> class.
        /// </summary>
        /// <param name="minimum">The minimum date expression.</param>
        /// <param name="maximum">The maximum date expression.</param>
        public RangeAttribute(string minimum, string maximum) {
            Minimum = minimum;
            Maximum = maximum;

            // Default fallback message if no resource is provided.
            // {0} = Field Name, {1} = Min Date, {2} = Max Date
            ErrorMessage = "The field {0} must be between {1} and {2}.";
        }

        /// <summary>
        ///     Applies formatting to an error message, replacing the placeholders with the field name 
        ///     and the calculated date limits.
        ///     This method is called by the framework when validation fails.
        /// </summary>
        /// <param name="name">The name to include in the formatted message (placeholder {0}).</param>
        /// <returns>An instance of the formatted error message.</returns>
        public override string FormatErrorMessage(string name) {
            // 1. Calculate the dynamic dates at the moment the error is generated.
            //    We do this here to ensure the message reflects the exact time of validation.
            var minDate = ParseDateExpression(Minimum);
            var maxDate = ParseDateExpression(Maximum);

            // 2. Retrieve the template string.
            //    If ErrorMessageResourceType/Name are set, 'base.ErrorMessageString' 
            //    automatically retrieves the localized string from the resource manager.
            var template = base.ErrorMessageString;

            // 3. Format the string using the current culture.
            //    We inject the field name ({0}), min date ({1}), and max date ({2}).
            return string.Format(CultureInfo.CurrentCulture, template, name, minDate.ToShortDateString(), maxDate.ToShortDateString());
        }

        /// <summary>
        ///     Validates the specified value with respect to the current system time.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        ///     An instance of the <see cref="ValidationResult"/> class.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            if (value is null) return ValidationResult.Success;

            if (value is not DateTime dateValue) {
                return new ValidationResult("The field must be a valid DateTime.");
            }

            var minDate = ParseDateExpression(Minimum);
            var maxDate = ParseDateExpression(Maximum);

            if (dateValue < minDate || dateValue > maxDate) {
                // Instead of creating the string manually here, we delegate to FormatErrorMessage
                // which handles the localization logic defined in the base class.
                return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
            }

            return ValidationResult.Success;
        }

        /// <summary>
        ///     Parses a string expression into a specific DateTime.
        /// </summary>
        protected virtual DateTime ParseDateExpression(string expression) {
            if (string.IsNullOrWhiteSpace(expression)) return DateTime.MinValue;

            var now = DateTime.Now;
            expression = expression.Trim();

            // Handle "Now" and offsets
            if (expression.StartsWith("Now", StringComparison.OrdinalIgnoreCase)) {
                if (expression.Equals("Now", StringComparison.OrdinalIgnoreCase)) return now;

                var match = Regex.Match(expression, @"Now([+\-])(\d+)([dmy])", RegexOptions.IgnoreCase);
                if (match.Success) {
                    var sign = match.Groups[1].Value == "+" ? 1 : -1;
                    var amount = int.Parse(match.Groups[2].Value) * sign;
                    var unit = match.Groups[3].Value.ToLower();

                    return unit switch {
                        "d" => now.AddDays(amount),
                        "m" => now.AddMonths(amount),
                        "y" => now.AddYears(amount),
                        _ => now
                    };
                }
            }

            // Handle fixed dates
            if (DateTime.TryParse(expression, CultureInfo.InvariantCulture, DateTimeStyles.None, out var staticDate)) {
                return staticDate;
            }

            throw new ArgumentException($"Invalid date expression: '{expression}'");
        }
    }
}
