using System;
using System.ComponentModel.DataAnnotations;
using GPSoftware.Core.SSN;

namespace GPSoftware.Core.Validation {

    /// <summary>
    ///     Validates that a string property conforms to one or more specified Social Security Number (SSN) formats
    ///     from different countries.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class SocialSecurityNumberAttribute : DataTypeAttribute {

        /// <summary>
        /// Defines the types of Social Security Numbers that can be validated.
        /// This enum uses the <see cref="FlagsAttribute"/> to allow combining multiple types.
        /// </summary>
        [Flags]
        public enum Types {
            Italian = 1 << 0,     // 0x01
            Swiss = 1 << 1,       // 0x02
            Austrian = 1 << 2,    // 0x04
            French = 1 << 3,      // 0x08
            Spanish = 1 << 4,     // 0x10
            UK = 1 << 5,          // 0x20
            Portuguese = 1 << 6,  // 0x40

            /// <summary>
            /// Represents any of the supported formats.
            /// </summary>
            Any = Italian | Swiss | Austrian | French | Spanish | UK | Portuguese
        }

        /// <summary>
        ///     Gets the types of Social Security Numbers that this attribute will validate against.
        /// </summary>
        public Types AcceptedTypes { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SocialSecurityNumberAttribute"/> class.
        /// </summary>
        public SocialSecurityNumberAttribute(Types types)
            : base(DataType.Text) {
            AcceptedTypes = types;
            ErrorMessage = "The field {0} must be a valid Social Security Number for the specified countries.";
        }

        /// <summary>
        ///     Determines whether the specified string is a valid SSN.
        /// </summary>
        public override bool IsValid(object? value) {
            // Allow null and empty to let [Required] handle it
            if (value == null) return true;
            if (!(value is string stringValue)) return false;
            if (string.IsNullOrEmpty(stringValue)) return true;

            return (((AcceptedTypes & Types.Italian) != 0) && SocialSecurityNumbers.IsValidCodiceFiscale(stringValue))
                || (((AcceptedTypes & Types.Swiss) != 0) && SocialSecurityNumbers.IsValidSwissAVS(stringValue))
                || (((AcceptedTypes & Types.Austrian) != 0) && SocialSecurityNumbers.IsValidAustrianSVNR(stringValue))
                || (((AcceptedTypes & Types.French) != 0) && SocialSecurityNumbers.IsValidFrenchINSEE(stringValue))
                || (((AcceptedTypes & Types.Spanish) != 0) && SocialSecurityNumbers.IsValidSpanishNIF(stringValue))
                || (((AcceptedTypes & Types.UK) != 0) && SocialSecurityNumbers.IsValidUKNINO(stringValue))
                || (((AcceptedTypes & Types.Portuguese) != 0) && SocialSecurityNumbers.IsValidPortugueseNIF(stringValue));
        }
    }
}
