using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GPSoftware.Core.Validation {

    /// <summary>
    ///     Phone number in the form [+]prefix ["blank"|.|-]number possible ending with ")"
    ///     Valid: +39.347.1314-799; 123456; 347-1314(799); +39 (123) 456 799
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PhoneNumberAttribute : DataTypeAttribute {

        private const string pattern = @"^\+?\d+[\d -.()]*[\d)]{1,1}$";

        public PhoneNumberAttribute()
            : base(DataType.Text) {
        }

        public override bool IsValid(object? value) {
            return (value == null) || ((value as string) == string.Empty) || Regex.IsMatch((value as string)!, pattern);
        }
    }
}
