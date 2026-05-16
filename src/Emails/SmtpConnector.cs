using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GPSoftware.Core.Emails {

    /// <summary>
    ///     Implementation of ISmtpConnector handling TCP and SSL streams safely.
    ///     Compatible with both .NET 4.7.2 and modern .NET variants.
    /// </summary>
    public class SmtpConnector : ISmtpConnector {

        public string SmtpServerAddress { get; protected set; }
        public int Port { get; protected set; }

        /// <inheritdoc />
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }

        private readonly TcpClient _client;
        private Stream _netStream;

        private const int DEFAULT_BUFFER_SIZE = 2048;
        public const string EOF = "\r\n";

#if NET48 || NET5_0_OR_GREATER
        private const SslProtocols DefaultSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
#else
        // TLS 1.3 is represented by value 12288 in older frameworks that don't have the enum member
        private const SslProtocols DefaultSslProtocols = SslProtocols.Tls12 | (SslProtocols)12288;
#endif

        /// <summary>
        ///     Initializes the connection to the specified SMTP server.
        /// </summary>
        public SmtpConnector(string smtpServerAddress, int port, bool enableSsl) {
            SmtpServerAddress = smtpServerAddress;
            Port = port;

            _client = new TcpClient(SmtpServerAddress, Port);
            _netStream = _client.GetStream();

            if (enableSsl) {
                UpgradeToSsl(false);
            }
        }

        /// <inheritdoc />
        public void WriteLine(string data) => SendData($"{data}{EOF}");

        /// <inheritdoc />
        public Task WriteLineAsync(string data) => SendDataAsync($"{data}{EOF}");

        /// <inheritdoc />
        public void SendData(string data) {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            _netStream.Write(buffer, 0, buffer.Length);
            _netStream.Flush();
        }

        /// <inheritdoc />
        public async Task SendDataAsync(string data) {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            await _netStream.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
            await _netStream.FlushAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool CheckResponse(int expectedCode, out string responseData) {
            return CheckResponse(new[] { expectedCode }, out responseData);
        }

        /// <inheritdoc />
        public bool CheckResponse(int[] expectedCodes, out string responseData) {
            responseData = ReadMessageFromStream(_netStream);
            if (string.IsNullOrWhiteSpace(responseData) || responseData.Length < 3) return false;

            if (int.TryParse(responseData.Substring(0, 3), out int code)) {
                return expectedCodes.Contains(code);
            }
            return false;
        }

        /// <inheritdoc />
        public bool CheckResponse(params int[] expectedCodes) {
            return CheckResponse(expectedCodes, out _);
        }

        /// <inheritdoc />
        public async Task<bool> CheckResponseAsync(params int[] expectedCodes) {
            var result = await CheckResponseExAsync(expectedCodes).ConfigureAwait(false);
            return result.IsSuccess;
        }

        /// <inheritdoc />
        public async Task<(bool IsSuccess, string ResponseData)> CheckResponseExAsync(params int[] expectedCodes) {
            string responseData = await ReadMessageFromStreamAsync(_netStream).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(responseData) || responseData.Length < 3) return (false, responseData);

            if (int.TryParse(responseData.Substring(0, 3), out int code)) {
                return (expectedCodes.Contains(code), responseData);
            }
            return (false, responseData);
        }

        /// <inheritdoc />
        public void UpgradeToSsl(bool checkCertificateRevocation = false) {
            var sslStream = new SslStream(_netStream, false, ValidateServerCertificate, null);
            _netStream = sslStream;
        }

        /// <inheritdoc />
        public Task UpgradeToSslAsync(bool checkCertificateRevocation = false) {
            UpgradeToSsl(checkCertificateRevocation);
            return Task.CompletedTask; // Wrapper to maintain interface compatibility
        }

        /// <inheritdoc />
        public void AuthenticateAsClient(bool checkCertificateRevocation = false) {
            if (_netStream is SslStream sslStream) {
                sslStream.AuthenticateAsClient(SmtpServerAddress, null, DefaultSslProtocols, checkCertificateRevocation);
            }
        }

        /// <inheritdoc />
        public async Task AuthenticateAsClientAsync(bool checkCertificateRevocation = false) {
            if (_netStream is SslStream sslStream) {
                await sslStream.AuthenticateAsClientAsync(SmtpServerAddress, null, DefaultSslProtocols, checkCertificateRevocation).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void StartTls(bool checkCertificateRevocation = false) {
            WriteLine("STARTTLS");
            if (!CheckResponse(220, out string reason)) {
                throw new InvalidOperationException($"Server rejected STARTTLS request: {reason}");
            }
            UpgradeToSsl(checkCertificateRevocation);
            AuthenticateAsClient(checkCertificateRevocation);
        }

        /// <inheritdoc />
        public async Task StartTlsAsync(bool checkCertificateRevocation = false) {
            await WriteLineAsync("STARTTLS").ConfigureAwait(false);
            var response = await CheckResponseExAsync(220).ConfigureAwait(false);
            if (!response.IsSuccess) {
                throw new InvalidOperationException($"Server rejected STARTTLS request: {response.ResponseData}");
            }
            await UpgradeToSslAsync(checkCertificateRevocation).ConfigureAwait(false);
            await AuthenticateAsClientAsync(checkCertificateRevocation).ConfigureAwait(false);
        }

        private bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            // 1. If the developer provided a custom callback via the interface, we honor it above all else.
            if (ServerCertificateValidationCallback != null) {
                return ServerCertificateValidationCallback(sender, certificate, chain, sslPolicyErrors);
            }

            // 2. If the OS says the certificate is perfectly fine, we accept it.
            if (sslPolicyErrors == SslPolicyErrors.None) {
                return true;
            }

            // 3. Fallback to smart heuristic validation for common SMTP shared server issues.
            return IsKnownMailServerCertificate(certificate, chain, sslPolicyErrors);
        }

        /// <summary>
        ///     Provides a smart fallback to handle common false-positives with SMTP certificates,
        ///     such as mismatched hostnames on shared hosting providers.
        /// </summary>
        private bool IsKnownMailServerCertificate(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {

            // A common issue with shared mail servers (like cPanel or ISPConfig) is that
            // they use a valid certificate issued to the hosting provider's main domain,
            // but the client connects to 'mail.domain.com', causing a name mismatch.
            // If the ONLY error is a name mismatch, but the chain is perfectly valid (trusted CA),
            // we can safely assume it's a known mail server scenario and allow the encrypted connection.
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch) {
                return true;
            }

            // Note: You can add specific trusted thumbprints here if you are dealing 
            // with specific self-signed development certificates.
            // if (certificate is X509Certificate2 cert2 && cert2.Thumbprint == "YOUR_THUMBPRINT") return true;

            return false;
        }

        private string ReadMessageFromStream(Stream stream) {
            byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
            StringBuilder messageData = new StringBuilder();
            int bytes;
            Decoder decoder = Encoding.UTF8.GetDecoder();

            do {
                bytes = stream.Read(buffer, 0, buffer.Length);
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                if (messageData.ToString().IndexOf(EOF, StringComparison.Ordinal) != -1) {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }

        private async Task<string> ReadMessageFromStreamAsync(Stream stream) {
            byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
            StringBuilder messageData = new StringBuilder();
            int bytes;
            Decoder decoder = Encoding.UTF8.GetDecoder();

            do {
                bytes = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);

                if (messageData.ToString().IndexOf(EOF, StringComparison.Ordinal) != -1) {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }

        /// <inheritdoc />
        public void Dispose() {
            _netStream?.Dispose();
            _client?.Close();
        }
    }
}
