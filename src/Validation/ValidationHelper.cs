using GPSoftware.Core.Extensions;

namespace GPSoftware.Core.Validation {

    public static class ValidationHelper {
        public static bool IsEmail(string value) => value.IsEmail();
    }
}