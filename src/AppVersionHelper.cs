using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using GPSoftware.Core.Extensions;

namespace GPSoftware.Core {

    /// <summary>
    /// Central point for application version.
    /// </summary>
    public class AppVersionHelper {
        private readonly Assembly _assembly;
        private readonly FileVersionInfo _versionInfo;
        private readonly FileInfo _info;

        private static AppVersionHelper? s_instance;

        /// <summary>
        ///
        /// </summary>
        /// <param name="assembly"></param>
        private AppVersionHelper(Assembly assembly) {
            _assembly = assembly;
            _info = new FileInfo(assembly.Location);
            _versionInfo = FileVersionInfo.GetVersionInfo(_assembly.Location);
        }

        public static AppVersionHelper GetInstance(Assembly assembly) {
            if (s_instance?._assembly != assembly) {
                s_instance = new AppVersionHelper(assembly);
            }
            return s_instance;
        }

        /// <summary>
        /// Gets current version of the application or null if the file did not contain version informatio
        /// </summary>
        public string? Version {
            get {
                return _versionInfo.ProductVersion;
            }
        }

        /// <summary>
        /// Gets release (last build) date of the application.
        /// It's shown in the web page.
        /// </summary>
        public DateTime ReleaseDate {
            get {
                return _info.LastWriteTime;
            }
        }

        public string AssemblyTitle {
            get {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0) {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != string.Empty) {
                        return titleAttribute.Title;
                    }
                }
                return "n/a";
            }
        }

        public string? AssemblyVersion {
            get {
                return _assembly.GetName().Version?.ToString();
            }
        }

        /// <summary>
        ///     Return the short version of the assembly version
        /// </summary>
        public string? AssemblyVersionShort {
            get {
                var version = this.AssemblyVersion;
                return version?.SubstringOrEmpty(0, version.LastIndexOf('.'));
            }
        }

        public string AssemblyDescription {
            get {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0) {
                    return string.Empty;
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct {
            get {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0) {
                    return string.Empty;
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright {
            get {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0) {
                    return string.Empty;
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany {
            get {
                object[] attributes = _assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0) {
                    return string.Empty;
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
    }
}