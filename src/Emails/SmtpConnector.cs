using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace GPSoftware.Core.Emails {

    public class SmtpConnector : ISmtpConnector, IDisposable {
        public string SmtpServerAddress { get; protected set; }
        public int Port { get; protected set; }

        private TcpClient? _client;
        private Stream? _netStream;
        
        // IAsyncResult is needed only if we use the old BeginConnect pattern
        private IAsyncResult? _connectionResult;

        /// <summary>Connection timeout in seconds</summary>
        private const byte CONNECT_TIMEOUT = 2;

        private const int DEFAULT_BUFFER_SIZE = 2048;
        public const string EOF = "\r\n";

#if NET48 || NET5_0_OR_GREATER
        private const SslProtocols DefaultSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
#else
        private const SslProtocols DefaultSslProtocols = SslProtocols.Tls12 | (SslProtocols)12288;
#endif

        #region Constructors

        /// <summary>
        ///     Open the smtp connection
        /// </summary>
        /// <param name="smtpServerAddress"></param>
        /// <param name="port"></param>
        /// <param name="enableSsl">if true, immediately open the connection using a secure protocol</param>
        public SmtpConnector(string smtpServerAddress, int port, bool enableSsl) {
            SmtpServerAddress = smtpServerAddress;
            Port = port;

            //TcpClient client = null;
            try {
                _client = new TcpClient();
                _connectionResult = _client.BeginConnect(SmtpServerAddress, Port, null, null);
                var success = _connectionResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(CONNECT_TIMEOUT));
                if (!success) {
                    throw new SmtpException($"Cannot connect to {smtpServerAddress}:{Port}");
                }
                // Complete the connection
                _client.EndConnect(_connectionResult);

                if (enableSsl) {
                    // create the ssl stream
                    _netStream = new SslStream(
                        innerStream: _client.GetStream(),
                        leaveInnerStreamOpen: false,
                        userCertificateValidationCallback: new RemoteCertificateValidationCallback(ValidateServerCertificate),
                        userCertificateSelectionCallback: null);
                } else {
                    // keep it clear
                    _netStream = _client.GetStream();
                }
            } catch {
                // Cleanup on failure
                Dispose();
                throw;
            }
        }

        // Finalizer calls Dispose(false)
        ~SmtpConnector() {
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // Free managed resources
                try {
                    _netStream?.Close();
                    _netStream?.Dispose();
                } catch { /* Ignored */ }
                
                try {
                    _client?.Close();
                    _client?.Dispose();
                } catch { /* Ignored */ }
            }

            // Free unmanaged resources (if any) and set large fields to null
            _netStream = null;
            _client = null;
        }

        #endregion Constructors

        /// <summary>
        ///     If current connesion is not secure, upgrade it and perform the authentication as client
        /// </summary>
        /// <param name="checkCertificateRevocation"></param>
        public virtual void UpgradeToSsl(bool checkCertificateRevocation = false) {
            if (_client == null) throw new InvalidOperationException("Client is not connected.");

            if (!(_netStream is SslStream)) {
                // create the ssl stream
                _netStream = new SslStream(
                    innerStream: _client.GetStream(),
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    userCertificateSelectionCallback: null);
            }
            AuthenticateAsClient(checkCertificateRevocation);
        }

        /// <summary>
        ///     If current connesion is not secure, upgrade it and perform the authentication as client
        /// </summary>
        /// <param name="checkCertificateRevocation"></param>
        public virtual Task UpgradeToSslAsync(bool checkCertificateRevocation = false) {
            if (_client == null) throw new InvalidOperationException("Client is not connected.");

            if (!(_netStream is SslStream)) {
                // create the ssl stream
                _netStream = new SslStream(
                    innerStream: _client.GetStream(),
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    userCertificateSelectionCallback: null);
            }
            return AuthenticateAsClientAsync(checkCertificateRevocation);
        }

        /// <summary>
        ///     Called by clients to authenticate the server and optionally the client in a client-server connection.
        ///     Throw an exception in case of error
        /// </summary>
        public void AuthenticateAsClient(bool checkCertificateRevocation = false) {
            if (_netStream is SslStream sslStream) {
                sslStream.AuthenticateAsClient(
                    targetHost: SmtpServerAddress,
                    clientCertificates: new X509CertificateCollection(),
                    enabledSslProtocols: DefaultSslProtocols,
                    checkCertificateRevocation: checkCertificateRevocation);
            } else {
                throw new SmtpException("Cannot authenticate over a non SSL connection");
            }
        }

        /// <summary>
        ///     Called by clients to authenticate the server and optionally the client in a client-server connection.
        ///     Throw an exception in case of error
        /// </summary>
        public Task AuthenticateAsClientAsync(bool checkCertificateRevocation = false) {
            if (_netStream is SslStream sslStream) {
                return sslStream.AuthenticateAsClientAsync(
                            targetHost: SmtpServerAddress,
                            clientCertificates: new X509CertificateCollection(),
                            enabledSslProtocols: DefaultSslProtocols,
                            checkCertificateRevocation: checkCertificateRevocation);
            } else {
                throw new SmtpException("Cannot authenticate over a non SSL connection");
            }
        }

        #region Check responses

        /// <summary>
        ///     Detects if response is valid by checking it returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes">the return codes expected from the SMTP server</param>
        public virtual bool CheckResponse(params int[] expectedCodes) {
            return CheckResponse(expectedCodes, out _);
        }

        /// <summary>
        ///     Detects if response is valid by checking it returns the passed <paramref name="expectedCode"/>
        /// </summary>
        /// <param name="expectedCode">the return code expected from the SMTP server</param>
        /// <param name="responseData">The response returned by the remote SMTP server</param>
        public virtual bool CheckResponse(int expectedCode, out string responseData) {
            return CheckResponse(new int[] { expectedCode }, out responseData);
        }

        /// <summary>
        ///     Check the response from the server returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes">the return codes expected from the SMTP server</param>
        /// <param name="responseData">The response returned by the remote SMTP server</param>
        public virtual bool CheckResponse(int[] expectedCodes, out string responseData) {
            if (_netStream == null) {
                responseData = $"socket not available (null) while checking for {string.Join(" or ", expectedCodes)}";
                return false;
            }

            // FIXED: Do not call async method from sync context to avoid deadlocks. Use the sync implementation.
            responseData = ReadMessageFromStream(_netStream);
            
            var isSuccess = false;
            if (!string.IsNullOrEmpty(responseData)) {
                // Safe parsing
                if (int.TryParse(responseData.Substring(0, Math.Min(3, responseData.Length)), out int responseCode)) {
                     isSuccess = Array.Exists(expectedCodes, el => el == responseCode);
                }
            }
            
            if (!isSuccess) responseData = $"Received '{responseData}' while checking for {string.Join(" or ", expectedCodes)}";

            return isSuccess;
        }

        /// <summary>
        ///     Detects if response is valid by checking it returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        /// <param name="expectedCodes">the return codes expected from the SMTP server</param>
        public virtual async Task<bool> CheckResponseAsync(params int[] expectedCodes) {
            return (await CheckResponseExAsync(expectedCodes).ConfigureAwait(false)).IsSuccess;
        }

        /// <summary>
        ///     Check the response from the server returns one of the passed <paramref name="expectedCodes"/>
        /// </summary>
        public virtual async Task<(bool IsSuccess, string ResponseData)> CheckResponseExAsync(params int[] expectedCodes) {
            if (_netStream == null) {
                return (false, $"socket not available (null) while checking for {string.Join(" or ", expectedCodes)}");
            }

            var result = (
                IsSuccess: false,
                ResponseData: await ReadMessageFromStreamAsync(_netStream).ConfigureAwait(false)
                );
            if (!string.IsNullOrEmpty(result.ResponseData)) {
                 if (int.TryParse(result.ResponseData.Substring(0, Math.Min(3, result.ResponseData.Length)), out int responseCode)) {
                    result.IsSuccess = Array.Exists(expectedCodes, el => el == responseCode);
                 }
            }
            if (!result.IsSuccess) result.ResponseData = $"Received '{result.ResponseData}' while checking for {string.Join(" or ", expectedCodes)}";

            return result;
        }

        #endregion Check responses

        #region SendData

        public virtual void SendData(string data) {
            if (_netStream == null) return;

            byte[] dataArray = Encoding.UTF8.GetBytes(data);
            // Send message to the server synchronously
            _netStream.Write(dataArray, 0, dataArray.Length);
            _netStream.Flush();
        }

        public virtual async Task SendDataAsync(string data) {
            if (_netStream == null) return;
            
            byte[] dataArray = Encoding.UTF8.GetBytes(data);
            // Send hello message to the server.
            await _netStream.WriteAsync(dataArray, 0, dataArray.Length).ConfigureAwait(false);
            await _netStream.FlushAsync().ConfigureAwait(false);
        }

        #endregion SendData

        #region Certificates

        /// <summary>
        ///     This method is invoked by the RemoteCertificateValidationDelegate
        /// </summary>
        protected virtual bool ValidateServerCertificate(
              object sender,
              X509Certificate? certificate,
              X509Chain? chain,
              SslPolicyErrors sslPolicyErrors) {
            const SslPolicyErrors mask = SslPolicyErrors.RemoteCertificateNotAvailable | SslPolicyErrors.RemoteCertificateNameMismatch;

            if (sslPolicyErrors == SslPolicyErrors.None)
                // no error, certificate is valid
                return true;

            if ((sslPolicyErrors & mask) == 0) {
                // At this point, all that is left is SslPolicyErrors.RemoteCertificateChainErrors

                // If the problem is an untrusted root, then compare the certificate to a list of known mail server certificates.
                if (IsUntrustedRoot(chain) && certificate is X509Certificate2 certificate2) {
                    if (IsKnownMailServerCertificate(certificate2))
                        return true;
                }
            }

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        /// <summary>
        ///     Check the whole chain till the root and return true if there are no errors
        /// </summary>
        /// <param name="chain"></param>
        /// <returns></returns>
        private static bool IsUntrustedRoot(X509Chain? chain) {
            //foreach (var status in chain.ChainStatus) {
            //    if (status.Status == X509ChainStatusFlags.NoError || status.Status == X509ChainStatusFlags.UntrustedRoot) continue;
            //    return false;
            //}
            return chain?.ChainStatus.Any(
                        status => (status.Status != X509ChainStatusFlags.NoError && status.Status != X509ChainStatusFlags.UntrustedRoot))
                    ?? true;

        }

        private const string AppleCertificateIssuer = "C=US, S=California, O=Apple Inc., CN=Apple Public Server RSA CA 12 - G1";
        private const string GMailCertificateIssuer = "CN=GTS CA 1C3, O=Google Trust Services LLC, C=US";
        private const string OutlookCertificateIssuer = "CN=DigiCert Cloud Services CA-1, O=DigiCert Inc, C=US";
        private const string YahooCertificateIssuer = "CN=DigiCert SHA2 High Assurance Server CA, OU=www.digicert.com, O=DigiCert Inc, C=US";
        private const string GmxDotComCertificateIssuer = "CN=GeoTrust RSA CA 2018, OU=www.digicert.com, O=DigiCert Inc, C=US";
        private const string GmxDotNetCertificateIssuer = "CN=TeleSec ServerPass Extended Validation Class 3 CA, STREET=Untere Industriestr. 20, L=Netphen, PostalCode=57250, S=Nordrhein Westfalen, OU=T-Systems Trust Center, O=T-Systems International GmbH, C=DE";

        // Note: This method auto-generated by https://gist.github.com/jstedfast/7cd36a51cee740ed84b18435106eaea5
        internal static bool IsKnownMailServerCertificate(X509Certificate2 certificate) {
            var cn = certificate.GetNameInfo(X509NameType.SimpleName, false);
            var fingerprint = certificate.Thumbprint;
            var serial = certificate.SerialNumber;
            var issuer = certificate.Issuer;

            switch (cn) {
                case "imap.gmail.com":
                    switch (issuer) {
                        case GMailCertificateIssuer:
                            return (serial == "00B93BA153E99924FA0A0000000110361E" && fingerprint == "626F8E5304A6894F73EA34C1A5BA30E659430A2F") // Expires 1/10/2022 4:47:53 AM
                                || (serial == "3D490726264C73630A0000000119533C" && fingerprint == "663B1630FCAAFBC30038BC2623F7DA087482C0FD") // Expires 1/23/2022 9:54:50 PM
                                || (serial == "2265A3434A5225380A00000001224DAF" && fingerprint == "52A853B1121A4B1A7C5DBDBFD6CE39F068888A2B") // Expires 1/31/2022 3:11:05 AM
                                || (serial == "00EC67725FAF05E6FD0A0000000125FF83" && fingerprint == "FF388B1BC174CBC3069B8709AB0CD93AF8077E94"); // Expires 2/20/2022 10:08:30 PM
                        default:
                            return false;
                    }
                case "pop.gmail.com":
                    switch (issuer) {
                        case GMailCertificateIssuer:
                            return (serial == "00D10ECCD5085799F50A0000000108AD82" && fingerprint == "EDC8EFF7BABFA726874E1E5753D9A203DB9FB539") // Expires 12/26/2021 9:12:18 PM
                                || (serial == "00D1539A6D091B588C0A00000001103626" && fingerprint == "F6C4E61A5F0E8E0ECB6194F13ADF6B255D9CF16D") // Expires 1/10/2022 4:48:09 AM
                                || (serial == "34C1C63D8A354B9E0A0000000119533E" && fingerprint == "081431D1D4E703B9038F630F29CE43D1505E7DA6") // Expires 1/23/2022 9:55:03 PM
                                || (serial == "32D8D7A3EB3680B80A00000001224DB8" && fingerprint == "664F07AEDFC6D1BB25669D6470CC84371F30146D"); // Expires 1/31/2022 3:11:21 AM
                        default:
                            return false;
                    }
                case "smtp.gmail.com":
                    switch (issuer) {
                        case GMailCertificateIssuer:
                            return (serial == "6C04C830530304AA0A0000000110363A" && fingerprint == "28C09AAA6A21E3DDBC3DDD67FBF375AAEF61B0C9") // Expires 1/10/2022 4:49:30 AM
                                || (serial == "7A99E46AA12130370A00000001195354" && fingerprint == "57A74EA716DC96B74035A7C08CD9649FBF2D834A") // Expires 1/23/2022 9:56:15 PM
                                || (serial == "00BDF6AD1401715D6B0A00000001224DCA" && fingerprint == "4B4948C238114FC92F31C59E5B85C73D1E47BADB"); // Expires 1/31/2022 3:12:48 AM
                        default:
                            return false;
                    }
                case "outlook.com":
                    switch (issuer) {
                        case OutlookCertificateIssuer:
                            return (serial == "0CCAC32B0EF281026392B8852AB15642" && fingerprint == "CBAA1582F1E49AD1D108193B5D38B966BE4993C6") // Expires 1/21/2022 6:59:59 PM
                                || (serial == "0CE67C905DDE83B20E77606A636AB967" && fingerprint == "E295CCF7F125F70907C2E7F97EF0F5E7D5704DE6"); // Expires 10/23/2022 7:59:59 PM
                        default:
                            return false;
                    }
                case "imap.mail.me.com":
                    return issuer == AppleCertificateIssuer && serial == "2EC9B6B93C77A53D15405C47A9FBC3CF" && fingerprint == "A047B6AE5E0FF51CC216C1237A44529B0A4DB0D2"; // Expires 10/2/2022 3:51:56 PM
                case "smtp.mail.me.com":
                    return issuer == AppleCertificateIssuer && serial == "46A537AD83083BCCBDA20D1D8657F573" && fingerprint == "83AA1EF97EE9AC0EAD8B2C88C62C83F8EDBF2BDB"; // Expires 10/30/2022 4:11:38 PM
                case "*.imap.mail.yahoo.com":
                    return issuer == YahooCertificateIssuer && serial == "07E7B4CB914FFC7FB3E03105C9DA0BE1" && fingerprint == "D7D39A265E914ADC8B443BF24DB684354D50B000"; // Expires 3/16/2022 7:59:59 PM
                case "legacy.pop.mail.yahoo.com":
                    switch (issuer) {
                        case YahooCertificateIssuer:
                            return (serial == "09CC4977A4C14D4388D90CF6676385FE" && fingerprint == "7BA05AF724299FF0688842ADEF2837DE25F3C4FD") // Expires 12/22/2021 6:59:59 PM
                                || (serial == "03B1E9610E0E209A4EA8FC192EBF55D7" && fingerprint == "7C32F642167257B00E55A9C5DC3E35F1719193BD"); // Expires 5/18/2022 11:59:59 PM
                        default:
                            return false;
                    }
                case "smtp.mail.yahoo.com":
                    return issuer == YahooCertificateIssuer && serial == "096122E949C73D57587E904DE8EBE2BC" && fingerprint == "C38CA2874F6489686FAE148482325EC3D8763D81"; // Expires 4/13/2022 7:59:59 PM
                case "mout.gmx.com":
                    return issuer == GmxDotComCertificateIssuer && serial == "06206F2270494CD7AD11F2B17E286C2C" && fingerprint == "A7D3BCC363B307EC3BDE21269A2F05117D6614A8"; // Expires 7/12/2022 8:00:00 AM
                case "mail.gmx.com":
                    return issuer == GmxDotComCertificateIssuer && serial == "0719A4D33A18B550133DDA3253AF6C96" && fingerprint == "948B0C3FA22BC12C91EEE5B1631A6C41B4A01B9C"; // Expires 7/12/2022 8:00:00 AM
                case "mail.gmx.net":
                    return issuer == GmxDotNetCertificateIssuer && serial == "070E7CD59BB7AFD73E8A206219C4F011" && fingerprint == "E66DC8FE17C9A7718D17441CBE347D1D6F7BF3D2"; // Expires 5/3/2022 7:59:59 PM
                default:
                    return false;
            }
        }

        #endregion Certificates

        /// <summary>
        /// Native synchronous read to avoid "Sync-over-Async" deadlocks.
        /// </summary>
        private string ReadMessageFromStream(Stream stream) {
            byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            
            // Use local decoder to handle state properly
            Decoder decoder = Encoding.UTF8.GetDecoder();
            
            do {
                // Blocking read
                bytes = stream.Read(buffer, 0, buffer.Length);

                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                
                // Check for EOF.
                if (messageData.ToString().IndexOf(EOF) != -1) {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }

        private async Task<string> ReadMessageFromStreamAsync(Stream stream) {
            byte[] buffer = new byte[DEFAULT_BUFFER_SIZE];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            
            Decoder decoder = Encoding.UTF8.GetDecoder();

            do {
                bytes = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                
                // Check for EOF.
                if (messageData.ToString().IndexOf(EOF) != -1) {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
    }
}
