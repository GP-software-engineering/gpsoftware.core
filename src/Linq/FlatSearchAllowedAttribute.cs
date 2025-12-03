using System;

namespace GPSoftware.Core.Linq {

    /// <summary>
    ///     If set, the property will be included in a flat search of query
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FlatSearchAllowedAttribute : Attribute {
    }
}