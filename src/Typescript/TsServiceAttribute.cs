using System;

namespace GPSoftware.Core.Typescript {

    /// <summary>Attribute for mark a class as DTO to generate in typescript</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TsDTOAttribute : Attribute {
        public string TPrimaryKey {
            get; private set;
        }

        public TsDTOAttribute(string tPrimaryKey = "number") {
            TPrimaryKey = tPrimaryKey;
        }
    }

    /// <summary>Attribute for mark a class as DTO to generate in typescript</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TsServiceAttribute : Attribute {
        public string TPrimaryKey {
            get; private set;
        }

        public TsServiceAttribute(string tPrimaryKey = "number") {
            TPrimaryKey = tPrimaryKey;
        }
    }

    /// <summary>Attribute for mark a class as DTO to generate in typescript</summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class TsEnumAttribute : Attribute {
    }

    /// <summary>Attribute for mark a property of type any during typescript generation</summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TsAnyAttribute : Attribute {
    }

    /// <summary>
    ///     This attribute allow to define a custom type for a property when genereting the .d.ts file.
    ///     E.g.
    ///         [TsCustom("mCcustomType")]
    ///         public MyCustomType MyProp { get; private set; }
    ///     generates:
    ///         myProp: myCustomType; 
    ///     
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TsCustomAttribute : Attribute {
        public string TsType {
            get; private set;
        }

        public TsCustomAttribute(string tsType) {
            TsType = tsType;
        }
    }

    /// <summary>Attribute for for skipping typescript generation</summary>
    [AttributeUsage(AttributeTargets.All)]
    public class TsIgnoreAttribute : Attribute {
    }

    /// <summary>
    ///     This attribute is used on a enum values to make that value usable only by authorized users.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class TsAuthorizeAttribute : Attribute {

        /// <summary>
        /// A list of permissions to authorize.
        /// </summary>
        public string[] Permissions {
            get; private set;
        }

        /// <summary>
        /// Creates a new instance of <see cref="TsAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="permissions">A list of permissions to authorize</param>
        public TsAuthorizeAttribute(params string[] permissions) {
            Permissions = permissions;
        }

        //public override string ToString() {
        //    StringBuilder str = new StringBuilder();
        //    foreach (var item in Permissions) {
        //        str.AppendFormat("\"{0}\", ", item);
        //    }
        //    return str.ToString();
        //}
    }

    /// <summary>
    ///     This attribute is used on a enum values to make that value usable only by authorized users.
    /// </summary>
    [AttributeUsage(/* AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Enum | */ AttributeTargets.All)]
    public class TsMasterKeysAttribute : Attribute {

        //public Type MasterFKType { get; private set; }
        public string[] MasterFKValues {
            get; private set;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="TsMasterKeysAttribute"/> class.
        /// </summary>
        /// <param name="masterFKvalues">A list of permissions to authorize</param>
        public TsMasterKeysAttribute(params string[] masterFKvalues) {
            MasterFKValues = masterFKvalues;
        }

        //public override string ToString() {
        //    StringBuilder str = new StringBuilder();
        //    foreach (var item in Permissions) {
        //        str.AppendFormat("\"{0}\", ", item);
        //    }
        //    return str.ToString();
        //}
    }
}