using GPSoftware.Core.SSN;

namespace System.ComponentModel.DataAnnotations {

    /// <summary>
    /// Validates that a string property conforms to one or more specified Social Security Number (SSN) formats
    /// from different countries.
    /// </summary>
    /// <remarks>
    /// This attribute allows specifying multiple SSN types (e.g., Italian, Swiss) using a flags enum.
    /// The validation will pass if the input string matches at least one of the specified SSN types.
    /// It relies on external validation methods in a <c>SocialSecurityNumbers</c> class (not provided here)
    /// for the actual format checking of each country's SSN.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class SocialSecurityNumberAttribute : DataTypeAttribute {
        /// <summary>
        /// Defines the types of Social Security Numbers that can be validated.
        /// This enum uses the <see cref="FlagsAttribute"/> to allow combining multiple types.
        /// </summary>
        [Flags]
        public enum Types {
            /// <summary>
            /// Represents an Italian Social Security Number (Codice Fiscale).
            /// </summary>
            Italian = 0x01,

            /// <summary>
            /// Represents a Swiss Social Security Number (AVS/AHV Number).
            /// </summary>
            Swiss = 0x02,

            /// <summary>
            /// Represents an Austrian Social Security Number (SVNR).
            /// </summary>
            Austrian = 0x04,

            /// <summary>
            /// Represents a French Social Security Number (INSEE Number).
            /// </summary>
            French = 0x08,

            // Future SSN types can be added here, e.g., German = 0x10, Spanish = 0x20, etc.

            /// <summary>
            /// Represents a French Social Security Number (INSEE Number).
            /// </summary>
            Any = 0xFF,
        }

        /// <summary>
        ///     Gets the types of Social Security Numbers that this attribute will validate against.
        /// </summary>
        public Types AcceptedTypes {
            get;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SocialSecurityNumberAttribute"/> class with the specified SSN types to validate against.
        /// </summary>
        /// <param name="types">
        ///     A bitwise combination of <see cref="Types"/> values indicating which country-specific SSN formats are considered valid.
        /// </param>
        public SocialSecurityNumberAttribute(Types types)
            : base(DataType.Text) // Treats the underlying data as text for basic data typing.
        {
            AcceptedTypes = types;
            ErrorMessage = "The field {0} must be a valid Social Security Number for the specified countries.";
        }

        /// <summary>
        ///     Determines whether the specified string is a valid SSN.
        /// </summary>
        /// <remarks>
        ///     If the input <paramref name="value"/> is null or empty, this validation returns <c>true</c>
        ///     because an SSN is expected to have content.
        /// </remarks>
        public override bool IsValid(object? value) {
            // I want to allow null and let [Required] handle it: return true;
            if (value == null) return true;

            // Ensure the value is a string
            if (!(value is string)) return false;

            var stringValue = value as string;

            // I want to allow empty strings
            if (string.IsNullOrEmpty(stringValue)) return true;

            return (((AcceptedTypes & Types.Italian) != 0) && SocialSecurityNumbers.IsValidCodiceFiscale(stringValue))
                || (((AcceptedTypes & Types.Swiss) != 0) && SocialSecurityNumbers.IsValidSwissAVS(stringValue))
                || (((AcceptedTypes & Types.Austrian) != 0) && SocialSecurityNumbers.IsValidAustrianSVNR(stringValue))
                || (((AcceptedTypes & Types.French) != 0) && SocialSecurityNumbers.IsValidFrenchINSEE(stringValue))
            ;
        }
    }
}