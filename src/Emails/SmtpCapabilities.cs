using System;

namespace GPSoftware.Core.Emails {

    /// <summary>
    ///		Capabilities supported by an SMTP server.
    /// </summary>
    /// <remarks>
    ///		Capabilities are read as part of the response to the EHLO command that
    ///		is issued during the connection phase of the smtp client.
    /// </remarks>
    [Flags]
    public enum SmtpCapabilities : uint {

        /// <summary>
        /// The server does not support any additional extensions.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The server supports the SIZE extension.
        /// </summary>
        Size = 1 << 0,

        /// <summary>
        ///     The server supports the DSN extension (Delivery Status Notifications).
        /// </summary>
        Dsn = 1 << 1,

        /// <summary>
        ///     The server supports the ENHANCEDSTATUSCODES extension.
        /// </summary>
        EnhancedStatusCodes = 1 << 2,

        /// <summary>
        ///     The server supports the AUTH extension, allowing clients to authenticate.
        /// </summary>
        Authentication = 1 << 3,

        /// <summary>
        ///     The server supports the 8BITMIME extension.
        /// </summary>
        EightBitMime = 1 << 4,

        /// <summary>
        ///     The server supports the PIPELINING extension.
        /// </summary>
        Pipelining = 1 << 5,

        /// <summary>
        ///     The server supports the BINARYMIME extension.
        /// </summary>
        BinaryMime = 1 << 6,

        /// <summary>
        ///     The server supports the CHUNKING extension.
        /// </summary>
        Chunking = 1 << 7,

        /// <summary>
        ///     The server supports the STARTTLS extension, allowing clients to switch to an encrypted connection.
        /// </summary>
        StartTLS = 1 << 8,

        /// <summary>
        ///     The server supports the SMTPUTF8 extension.
        /// </summary>
        UTF8 = 1 << 9,

        /// <summary>
        ///     The server supports the REQUIRETLS extension.
        /// </summary>
        RequireTLS = 1 << 10
    }
}
