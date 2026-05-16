using System;
using System.Net.Security;
using System.Threading.Tasks;

namespace GPSoftware.Core.Emails {

    /// <summary>
    ///     Defines the low-level contract for interacting with an SMTP server over TCP and handling secure streams.
    /// </summary>
    public interface ISmtpConnector : IDisposable {

        /// <summary>
        ///     Gets or sets a custom callback to validate the server certificate.
        ///     If null, the default internal validation logic is applied.
        /// </summary>
        RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }

        /// <summary>
        ///     Checks if the SMTP server responded with the expected status code.
        /// </summary>
        /// <param name="expectedCode">The expected SMTP status code (e.g., 250).</param>
        /// <param name="responseData">The raw response data read from the stream.</param>
        /// <returns>True if the response code matches the expected code.</returns>
        bool CheckResponse(int expectedCode, out string responseData);

        /// <summary>
        ///     Checks if the SMTP server responded with any of the expected status codes.
        /// </summary>
        /// <param name="expectedCodes">An array of accepted SMTP status codes.</param>
        /// <param name="responseData">The raw response data read from the stream.</param>
        /// <returns>True if the response code is within the expected codes.</returns>
        bool CheckResponse(int[] expectedCodes, out string responseData);

        /// <summary>
        ///     Checks if the SMTP server responded with any of the expected status codes, discarding the response data.
        /// </summary>
        /// <param name="expectedCodes">A variable list of accepted SMTP status codes.</param>
        /// <returns>True if the response code is within the expected codes.</returns>
        bool CheckResponse(params int[] expectedCodes);

        /// <summary>
        ///     Asynchronously checks if the SMTP server responded with any of the expected status codes.
        /// </summary>
        /// <param name="expectedCodes">A variable list of accepted SMTP status codes.</param>
        /// <returns>A task representing the asynchronous operation, returning true if matched.</returns>
        Task<bool> CheckResponseAsync(params int[] expectedCodes);

        /// <summary>
        ///     Asynchronously checks if the SMTP server responded with the expected status codes and retrieves the raw response.
        /// </summary>
        /// <param name="expectedCodes">A variable list of accepted SMTP status codes.</param>
        /// <returns>A tuple containing the success status and the raw response string.</returns>
        Task<(bool IsSuccess, string ResponseData)> CheckResponseExAsync(params int[] expectedCodes);

        /// <summary>
        ///     Sends raw string data to the SMTP server over the network stream.
        /// </summary>
        /// <param name="data">The string data to send.</param>
        void SendData(string data);

        /// <summary>
        ///     Asynchronously sends raw string data to the SMTP server over the network stream.
        /// </summary>
        /// <param name="data">The string data to send.</param>
        Task SendDataAsync(string data);

        /// <summary>
        ///     Sends a string data line to the SMTP server, appending the standard CRLF (End Of File) sequence.
        /// </summary>
        /// <param name="data">The string data to send.</param>
        void WriteLine(string data);

        /// <summary>
        ///     Asynchronously sends a string data line to the SMTP server, appending the standard CRLF sequence.
        /// </summary>
        /// <param name="data">The string data to send.</param>
        Task WriteLineAsync(string data);

        /// <summary>
        ///     Upgrades the current plaintext TCP stream to a secure SSL/TLS stream.
        /// </summary>
        /// <param name="checkCertificateRevocation">If true, checks the certificate revocation list (CRL) during validation.</param>
        void UpgradeToSsl(bool checkCertificateRevocation = false);

        /// <summary>
        ///     Asynchronously upgrades the current plaintext TCP stream to a secure SSL/TLS stream.
        /// </summary>
        /// <param name="checkCertificateRevocation">If true, checks the certificate revocation list (CRL) during validation.</param>
        Task UpgradeToSslAsync(bool checkCertificateRevocation = false);

        /// <summary>
        ///     Authenticates the client over the established SSL/TLS stream.
        /// </summary>
        /// <param name="checkCertificateRevocation">If true, checks the certificate revocation list (CRL) during validation.</param>
        void AuthenticateAsClient(bool checkCertificateRevocation = false);

        /// <summary>
        ///     Asynchronously authenticates the client over the established SSL/TLS stream.
        /// </summary>
        /// <param name="checkCertificateRevocation">If true, checks the certificate revocation list (CRL) during validation.</param>
        Task AuthenticateAsClientAsync(bool checkCertificateRevocation = false);

        /// <summary>
        ///     Issues the STARTTLS command to elevate the connection, upgrades the stream, and authenticates as a client.
        /// </summary>
        /// <param name="checkCertificateRevocation">If true, checks the certificate revocation list (CRL) during validation.</param>
        void StartTls(bool checkCertificateRevocation = false);

        /// <summary>
        ///     Asynchronously issues the STARTTLS command to elevate the connection, upgrades the stream, and authenticates as a client.
        /// </summary>
        /// <param name="checkCertificateRevocation">If true, checks the certificate revocation list (CRL) during validation.</param>
        Task StartTlsAsync(bool checkCertificateRevocation = false);
    }
}
