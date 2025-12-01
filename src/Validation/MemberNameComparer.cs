using System.Collections.Generic;

namespace GPSoftware.Core.Validation {

    /// <summary>
    ///     See my issue on AbpBoilerplate
    /// </summary>
    /// <see cref="https://github.com/aspnetboilerplate/aspnetboilerplate/issues/1946"/>
    public class MemberNameComparer : IEqualityComparer<string> {

        /// <summary>Static instance for performance)</summary>
        public static readonly MemberNameComparer Default = new MemberNameComparer();

        public bool Equals(string x, string y) {
            return x.Equals(y) || x.EndsWith(_PREFIX + y);
        }

        public int GetHashCode(string x) {
            return x.GetHashCode();
        }

        private const string _PREFIX = ".";
    }
}