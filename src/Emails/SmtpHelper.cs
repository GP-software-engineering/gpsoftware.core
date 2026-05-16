using System;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPSoftware.Core.Emails {

    /// <summary>
    ///     Helper methods to check mail server capabilities, validate credentials, 
    ///     and manage SMTP connections safely without blocking application threads.
    ///     Optimized for both .NET Framework 4.7.2 and modern .NET 6+ environments.
    /// </summary>
    public static class SmtpHelper {

        /// <summary>
        ///     Connects to a mail server, reads the greeting, and sends an EHLO command. 
        ///     Returns true if the server replies positively.
        ///     Reference: https://www.greenend.org.uk/rjk/tech/smtpreplies.html
        /// </summary>
        /// <param name="server">The SMTP server hostname or IP address.</param>
        /// <param name="port">The SMTP server port (e.g., 25, 465, 587).</param>
        /// <param name="secureMode">The security mode to use for the connection.</param>
        /// <returns>True if the EHLO command was successful; otherwise, false.</returns>
        public static bool EHLOcheck(string server, int port, SecureSocketMode secureMode) {
            return EHLOcheck(server, port, secureMode, out _);
        }

        /// <summary>
        ///     Connects to a mail server, reads the greeting, and sends an EHLO command. 
        ///     Outputs the server's reply reason.
        /// </summary>
        /// <param name="server">The SMTP server hostname or IP address.</param>
        /// <param name="port">The SMTP server port (e.g., 25, 465, 587).</param>
        /// <param name="secureMode">The security mode to use for the connection.</param>
        /// <param name="reason">The raw response or error message from the server.</param>
        /// <returns>True if the EHLO command was successful; otherwise, false.</returns>
        public static bool EHLOcheck(string server, int port, SecureSocketMode secureMode, out string reason) {
            ISmtpConnector connector = null;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        if (!ReadGreeting(connector, out reason)) return false;
                        return SendEhlo(connector, out reason);

                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(server, port, enableSsl: true);
                        connector.AuthenticateAsClient();
                        if (!ReadGreeting(connector, out reason)) return false;
                        return SendEhlo(connector, out reason);

                    case SecureSocketMode.StartTlsWhenAvailable:
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(server, port, enableSsl: false);

                        // Initial plain text greeting and EHLO
                        if (!ReadGreeting(connector, out reason)) return false;
                        if (!SendEhlo(connector, out reason)) return false;

                        // Check if STARTTLS is supported and required
                        if (GetSmtpCapabilities(reason).HasFlag(SmtpCapabilities.StartTLS)) {
                            connector.StartTls(); // Automatically negotiates the secure stream

                            // Send EHLO again over the secure channel (No 220 greeting is sent after TLS negotiation)
                            return SendEhlo(connector, out reason); 
                        } else {
                            if (secureMode == SecureSocketMode.StartTls) {
                                reason = "STARTTLS is not available on this server";
                                return false;
                            } else {
                                return true; // StartTlsWhenAvailable allows fallback to plain text
                            }
                        }

                    default:
                        reason = "Unknown SecureSocketMode";
                        return false;
                }
            } catch (Exception ex) {
                reason = ex.Message;
                return false;
            } finally {
                connector?.Dispose();
            }
        }

        /// <summary>
        ///     Reads the initial 220 connection greeting from the SMTP server.
        /// </summary>
        private static bool ReadGreeting(ISmtpConnector connector, out string reason) {
            return connector.CheckResponse(220, out reason);
        }

        /// <summary>
        ///     Sends the EHLO command using the local machine's HostName (RFC 2821 compliant) 
        ///     and checks for a 250 success response.
        /// </summary>
        private static bool SendEhlo(ISmtpConnector connector, out string reason) {
            string hostName = Dns.GetHostName();
            connector.WriteLine($"EHLO {hostName}");
            return connector.CheckResponse(250, out reason);
        }

        /// <summary>
        ///     Validates SMTP credentials by connecting, establishing a secure channel if required,
        /// </summary>
        /// <param name="userName">The authentication username.</param>
        /// <param name="password">The authentication password.</param>
        /// <param name="server">The SMTP server hostname or IP address.</param>
        /// <param name="port">The SMTP server port (e.g., 25, 465, 587).</param>
        /// <param name="secureMode">The security mode to use for the connection.</param>
        /// <param name="reason">The raw response or error message from the server.</param>
        /// <returns>True if authentication succeeded; otherwise, false.</returns>
        public static bool ValidateCredentials(string userName, string password, string server, int port, SecureSocketMode secureMode, out string reason) {
            ISmtpConnector connector = null;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        if (!ReadGreeting(connector, out reason)) return false;
                        if (!SendEhlo(connector, out reason)) return false;
                        break;

                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(server, port, enableSsl: true);
                        connector.AuthenticateAsClient();
                        if (!ReadGreeting(connector, out reason)) return false;
                        if (!SendEhlo(connector, out reason)) return false;
                        break;

                    case SecureSocketMode.StartTlsWhenAvailable:
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        
                        if (!ReadGreeting(connector, out reason)) return false;
                        if (!SendEhlo(connector, out reason)) return false;

                        if (GetSmtpCapabilities(reason).HasFlag(SmtpCapabilities.StartTLS)) {
                            connector.StartTls();
                            if (!SendEhlo(connector, out reason)) return false;
                        } else if (secureMode == SecureSocketMode.StartTls) {
                            reason = "STARTTLS is not available";
                            return false;
                        }
                        break;

                    default:
                        reason = "Unknown SecureSocketMode";
                        return false;
                }

                // Authentication phase (AUTH LOGIN)
                connector.WriteLine("AUTH LOGIN");
                if (!connector.CheckResponse(334, out reason)) return false;

                connector.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(userName)));
                if (!connector.CheckResponse(334, out reason)) return false;

                connector.WriteLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(password)));
                return connector.CheckResponse(235, out reason);

            } catch (Exception ex) {
                reason = ex.Message;
                return false;
            } finally {
                connector?.Dispose();
            }
        }

        /// <summary>
        ///     Parses the raw EHLO response string to determine the server's supported capabilities.
        /// </summary>
        /// <param name="ehloResponse">The raw response string received after sending EHLO.</param>
        /// <returns>A bitwise enum of the parsed <see cref="SmtpCapabilities"/>.</returns>
        public static SmtpCapabilities GetSmtpCapabilities(string ehloResponse) {
            var capabilities = SmtpCapabilities.None;
            var lines = ehloResponse.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            // Skip the first line which is just the server greeting "250-smtp.server.com Hello..."
            for (int i = 1; i < lines.Length; i++) {
                var line = lines[i];
                if (line.Length < 4) continue;

                // Capabilities usually start after the "250-" or "250 " prefix
                var capability = line.Substring(4).Trim().ToUpperInvariant();

                if (capability.StartsWith("AUTH", StringComparison.Ordinal)) {
                    capabilities |= SmtpCapabilities.Authentication;
                } else if (capability.StartsWith("SIZE", StringComparison.Ordinal)) {
                    capabilities |= SmtpCapabilities.Size;
                } else {
                    switch (capability) {
                        case "DSN": capabilities |= SmtpCapabilities.Dsn; break;
                        case "BINARYMIME": capabilities |= SmtpCapabilities.BinaryMime; break;
                        case "CHUNKING": capabilities |= SmtpCapabilities.Chunking; break;
                        case "ENHANCEDSTATUSCODES": capabilities |= SmtpCapabilities.EnhancedStatusCodes; break;
                        case "8BITMIME": capabilities |= SmtpCapabilities.EightBitMime; break;
                        case "PIPELINING": capabilities |= SmtpCapabilities.Pipelining; break;
                        case "STARTTLS": capabilities |= SmtpCapabilities.StartTLS; break;
                        case "SMTPUTF8": capabilities |= SmtpCapabilities.UTF8; break;
                        case "REQUIRETLS": capabilities |= SmtpCapabilities.RequireTLS; break;
                    }
                }
            }

            return capabilities;
        }

        // =========================================================================================
        // MODERN ASYNC EXTENSIONS
        // =========================================================================================

        /// <summary>
        ///     Asynchronously connects to a mail server, reads the greeting, and sends an EHLO command. 
        ///     Returns true if the server replies positively.
        /// </summary>
        public static async Task<bool> EHLOcheckAsync(string server, int port, SecureSocketMode secureMode) {
            var result = await EHLOcheckExAsync(server, port, secureMode).ConfigureAwait(false);
            return result.IsSuccess;
        }

        /// <summary>
        ///     Asynchronously connects to a mail server, reads the greeting, and sends an EHLO command. 
        ///     Returns the success status and the server's reply reason.
        /// </summary>
        public static async Task<(bool IsSuccess, string Reason)> EHLOcheckExAsync(string server, int port, SecureSocketMode secureMode) {
            ISmtpConnector connector = null;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        
                        var greetNone = await ReadGreetingAsync(connector).ConfigureAwait(false);
                        if (!greetNone.IsSuccess) return greetNone;
                        
                        return await SendEhloAsync(connector).ConfigureAwait(false);

                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(server, port, enableSsl: true);
                        await connector.AuthenticateAsClientAsync().ConfigureAwait(false);
                        
                        var greetSsl = await ReadGreetingAsync(connector).ConfigureAwait(false);
                        if (!greetSsl.IsSuccess) return greetSsl;
                        
                        return await SendEhloAsync(connector).ConfigureAwait(false);

                    case SecureSocketMode.StartTlsWhenAvailable:
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        
                        var greetTls = await ReadGreetingAsync(connector).ConfigureAwait(false);
                        if (!greetTls.IsSuccess) return greetTls;
                        
                        var ehloTls = await SendEhloAsync(connector).ConfigureAwait(false);
                        if (!ehloTls.IsSuccess) return ehloTls;

                        if (GetSmtpCapabilities(ehloTls.Reason).HasFlag(SmtpCapabilities.StartTLS)) {
                            await connector.StartTlsAsync().ConfigureAwait(false);
                            return await SendEhloAsync(connector).ConfigureAwait(false); 
                        } else {
                            if (secureMode == SecureSocketMode.StartTls) {
                                return (false, "STARTTLS is not available on this server");
                            } else {
                                return (true, ehloTls.Reason);
                            }
                        }

                    default:
                        return (false, "Unknown SecureSocketMode");
                }
            } catch (Exception ex) {
                return (false, ex.Message);
            } finally {
                connector?.Dispose();
            }
        }

        /// <summary>
        ///     Asynchronously validates SMTP credentials natively using the ISmtpConnector async methods.
        ///     This prevents Thread Pool starvation by fully yielding the thread during network I/O.
        /// </summary>
        public static async Task<bool> ValidateCredentialsAsync(string userName, string password, string server, int port, SecureSocketMode secureMode) {
            ISmtpConnector connector = null;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        if (!(await ReadGreetingAsync(connector).ConfigureAwait(false)).IsSuccess) return false;
                        if (!(await SendEhloAsync(connector).ConfigureAwait(false)).IsSuccess) return false;
                        break;

                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(server, port, enableSsl: true);
                        await connector.AuthenticateAsClientAsync().ConfigureAwait(false);
                        if (!(await ReadGreetingAsync(connector).ConfigureAwait(false)).IsSuccess) return false;
                        if (!(await SendEhloAsync(connector).ConfigureAwait(false)).IsSuccess) return false;
                        break;

                    case SecureSocketMode.StartTlsWhenAvailable:
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(server, port, enableSsl: false);
                        
                        if (!(await ReadGreetingAsync(connector).ConfigureAwait(false)).IsSuccess) return false;
                        
                        var ehloResult = await SendEhloAsync(connector).ConfigureAwait(false);
                        if (!ehloResult.IsSuccess) return false;

                        if (GetSmtpCapabilities(ehloResult.Reason).HasFlag(SmtpCapabilities.StartTLS)) {
                            await connector.StartTlsAsync().ConfigureAwait(false);
                            if (!(await SendEhloAsync(connector).ConfigureAwait(false)).IsSuccess) return false;
                        } else if (secureMode == SecureSocketMode.StartTls) {
                            return false; // STARTTLS is required but not available
                        }
                        break;

                    default:
                        return false;
                }

                // Authentication phase (AUTH LOGIN)
                await connector.WriteLineAsync("AUTH LOGIN").ConfigureAwait(false);
                if (!await connector.CheckResponseAsync(334).ConfigureAwait(false)) return false;

                await connector.WriteLineAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(userName))).ConfigureAwait(false);
                if (!await connector.CheckResponseAsync(334).ConfigureAwait(false)) return false;

                await connector.WriteLineAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(password))).ConfigureAwait(false);
                return await connector.CheckResponseAsync(235).ConfigureAwait(false);

            } catch {
                return false;
            } finally {
                connector?.Dispose();
            }
        }

        private static async Task<(bool IsSuccess, string Reason)> ReadGreetingAsync(ISmtpConnector connector) {
            return await connector.CheckResponseExAsync(220).ConfigureAwait(false);
        }

        private static async Task<(bool IsSuccess, string Reason)> SendEhloAsync(ISmtpConnector connector) {
            string hostName = Dns.GetHostName();
            await connector.WriteLineAsync($"EHLO {hostName}").ConfigureAwait(false);
            return await connector.CheckResponseExAsync(250).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends an email asynchronously with a strict hard-coded timeout safety wrapper.
        ///     This bypasses the architectural bug where SmtpClient.SendMailAsync ignores the Timeout property.
        /// </summary>
        /// <param name="smtpClient">The pre-configured SmtpClient instance.</param>
        /// <param name="mailMessage">The MailMessage object to send.</param>
        /// <param name="timeoutMilliseconds">The maximum allowed duration in milliseconds before forcing an abort.</param>
        public static async Task SendMailWithTimeoutAsync(this SmtpClient smtpClient, MailMessage mailMessage, int timeoutMilliseconds) {
            if (smtpClient == null) throw new ArgumentNullException(nameof(smtpClient));
            if (mailMessage == null) throw new ArgumentNullException(nameof(mailMessage));

            // Ensure the synchronous timeout property is also set as a primary fallback
            smtpClient.Timeout = timeoutMilliseconds;

#if NET6_0_OR_GREATER
            // In modern .NET 6+, we use the built-in WaitAsync for elegant and native timeouts
            using var cts = new CancellationTokenSource(timeoutMilliseconds);
            try {
                await smtpClient.SendMailAsync(mailMessage).WaitAsync(cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) {
                smtpClient.Dispose(); // Force-close the hanging underlying TCP socket
                throw new TimeoutException($"SMTP send operation timed out after {timeoutMilliseconds}ms.");
            }
#else
            // Legacy .NET 4.7.2 fallback using Task.WhenAny to enforce timeout safely
            var sendTask = smtpClient.SendMailAsync(mailMessage);
            var delayTask = Task.Delay(timeoutMilliseconds);

            var completedTask = await Task.WhenAny(sendTask, delayTask).ConfigureAwait(false);

            if (completedTask == delayTask) {
                // Brutally dispose the SmtpClient to force-close the underlying TCP socket connection immediately
                smtpClient.Dispose();
                throw new TimeoutException($"SMTP send operation timed out after {timeoutMilliseconds}ms.");
            }

            // Await the actual send task to correctly propagate any network/authentication exceptions
            await sendTask.ConfigureAwait(false);
#endif
        }

        /// <summary>
        ///     Safely checks if the remote SMTP server is reachable via raw TCP connection within a specified timeout.
        /// </summary>
        /// <param name="host">The SMTP server hostname or IP address.</param>
        /// <param name="port">The SMTP server port (e.g., 25, 465, 587).</param>
        /// <param name="timeoutMilliseconds">The connection timeout limit in milliseconds.</param>
        /// <returns>True if the connection was established successfully; otherwise, false.</returns>
        public static async Task<bool> RawPingSmtpServerAsync(string host, int port, int timeoutMilliseconds) {
            if (string.IsNullOrWhiteSpace(host)) return false;

            try {
                using (var tcpClient = new TcpClient()) {
#if NET6_0_OR_GREATER
                    // Modern .NET 6+ implementation uses CancellationToken directly on the socket
                    using var cts = new CancellationTokenSource(timeoutMilliseconds);
                    await tcpClient.ConnectAsync(host, port, cts.Token).ConfigureAwait(false);
                    return tcpClient.Connected;
#else
                    // Legacy .NET 4.7.2 fallback using Task.WhenAny to enforce timeout safely
                    var connectTask = tcpClient.ConnectAsync(host, port);
                    var delayTask = Task.Delay(timeoutMilliseconds);

                    var completedTask = await Task.WhenAny(connectTask, delayTask).ConfigureAwait(false);

                    if (completedTask == delayTask) {
                        return false; // Connection attempt timed out
                    }

                    // Propagate potential connection exceptions (e.g. SocketException)
                    await connectTask.ConfigureAwait(false);
                    return tcpClient.Connected;
#endif
                }
            } catch {
                // Connection failed due to network unreachable, firewall blocking, or timeout exception
                return false; 
            }
        }
    }
}
