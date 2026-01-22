using System;
using System.ComponentModel.DataAnnotations;

namespace GPSoftware.Core.Validation {

    /// <summary>
    ///     Validates that a <see cref="Guid"/> property is not <see cref="Guid.Empty"/>.
    /// </summary>
    /// <remarks>
    ///     This attribute is specifically designed to handle <see cref="Guid"/> types where the standard 
    ///     <see cref="RequiredAttribute"/> might succeed even if the value is <c>Guid.Empty</c> (00000000-0000-0000-0000-000000000000).
    /// </remarks>
    /// <example>
    ///     Usage in a DTO or Command class:
    /// <code>
    /// public class CreateUserCommand 
    /// {
    ///     [RequiredNonEmptyGuid(ErrorMessage = "The TenantId cannot be empty.")]
    ///     public Guid TenantId { get; set; }
    /// }
    /// </code>
    /// </example>    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class RequiredNonEmptyGuidAttribute : ValidationAttribute {

        public RequiredNonEmptyGuidAttribute() {
            // Default error key if none is provided
            ErrorMessage = "The {0} field is required.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) {
            // 1. Check Validity (Guid.Empty is invalid)
            bool isValid = false;

            if (value is Guid guid) {
                isValid = guid != Guid.Empty;
            } else if (value is null) {
                isValid = false;
            }

            return isValid ? ValidationResult.Success : new ValidationResult(ErrorMessage, new[] { validationContext.MemberName! });

            //if (isValid) {
            //    return ValidationResult.Success;
            //}

            //// 2. Resolve Localization Service
            //// We get the localizer specifically for AssetlyResource
            //var localizer = validationContext.GetService<IStringLocalizer<AssetlyResource>>();

            //string errorMessage = ErrorMessageString;

            //// 3. Try to localize the error message key
            //if (localizer != null && !string.IsNullOrEmpty(ErrorMessage)) {
            //    // We assume 'ErrorMessage' contains the Localization Key (e.g. "Security:ExchangeRequired")
            //    var localizedString = localizer[ErrorMessage, validationContext.MemberName!];

            //    // If resource is found, use it. Otherwise fall back to the key itself.
            //    if (!localizedString.ResourceNotFound) {
            //        errorMessage = localizedString.Value;
            //    }
            //}

            //// Return error with member name
            //return new ValidationResult(
            //    errorMessage,
            //    new[] { validationContext.MemberName! }
            //);
        }
    }
}
