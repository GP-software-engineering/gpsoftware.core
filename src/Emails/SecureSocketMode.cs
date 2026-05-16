using System;

namespace GPSoftware.Core.Emails {

    /// <summary>
    ///     Secure socket options.
    /// </summary>
    /// <remarks>
    ///     Provides a way of specifying the SSL and/or TLS encryption that should be used for a connection.
    /// </remarks>
    [Flags]
    public enum SecureSocketMode : uint {

        /// <summary>
        ///     No SSL or TLS encryption should be used.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The connection should use SSL encryption immediately (Implicit SSL, usually port 465).
        /// </summary>
        SslOnConnect = 1 << 0,

        /// <summary>
        ///     Elevates the connection to use TLS encryption immediately after reading the greeting and capabilities
        ///     of the server. If the server does not support the STARTTLS extension, then the connection will fail.
        /// </summary>
        StartTls = 1 << 1,

        /// <summary>
        ///     Elevates the connection to use TLS encryption if the server supports it.
        ///     If not, the connection continues in plain text.
        /// </summary>
        StartTlsWhenAvailable = 1 << 2
    }
}
