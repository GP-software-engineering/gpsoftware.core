using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GPSoftware.Core.Emails {

    /// <summary>
    ///     Helper methods to check mail server capabilities
    /// </summary>
    public static class SmtpHelper {

        /// <summary>
        ///     Connect to a mail server and send an EHLO command. Return the server's reply.
        ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
        /// </summary>
        public static bool EHLOcheck(string server, int port, SecureSocketMode secureMode) {
            return EHLOcheck(server, port, secureMode, out _);
        }

        /// <summary>
        ///     Connect to a mail server and send an EHLO command. Return the server's reply.
        ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
        /// </summary>
        public static bool EHLOcheck(string server, int port, SecureSocketMode secureMode, out string reason) {
            ISmtpConnector connector;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(smtpServerAddress: server, port: port, enableSsl: false);
                        return DoEHLOcheck(connector, out reason);
                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(smtpServerAddress: server, port: port, enableSsl: true);
                        connector.AuthenticateAsClient();
                        return DoEHLOcheck(connector, out reason);
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(smtpServerAddress: server, port: port, enableSsl: false);
                        return DoStartTlsCheck(connector, out reason);
                    default:
                        throw new NotSupportedException(secureMode.ToString());
                }
            } catch (Exception ex) {
                reason = ex.Message;
                return false;
            }
        }

        /// <summary>
        ///     Connect to a mail server and send an EHLO command. Return true if all is ok.
        ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
        /// </summary>
        public static async Task<bool> EHLOcheckAsync(string server, int port, SecureSocketMode secureMode) {
            return (await EHLOcheckExAsync(server, port, secureMode).ConfigureAwait(false)).IsSuccess;
        }

        /// <summary>
        ///     Connect to a mail server and send an EHLO command. Return the server's reply.
        ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
        /// </summary>
        public static async Task<(bool IsSuccess, string Reason)> EHLOcheckExAsync(string server, int port, SecureSocketMode secureMode) {
            ISmtpConnector connector;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(server, port, false);
                        return await DoEHLOcheckAsync(connector);
                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(server, port, true);
                        await connector.AuthenticateAsClientAsync().ConfigureAwait(false);
                        return await DoEHLOcheckAsync(connector);
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(server, port, false);
                        return await DoStartTlsCheckExAsync(connector);
                    default:
                        throw new NotSupportedException(secureMode.ToString());
                }
            } catch (Exception ex) {
                return (false, ex.Message);
            }
        }

        /*
                /// <summary>
                ///     Connect to a mail server, send an EHLO command an check the STARTTLS capability. Return true if all is ok.
                ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
                /// </summary>
                public static bool StartTLScheck(string server, int port) {
                    return StartTLScheck(server, port, out _);
                }

                /// <summary>
                ///     Connect to a mail server, send an EHLO command an check the STARTTLS capability. Return true if all is ok.
                ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
                /// </summary>
                public static bool StartTLScheck(string server, int port, out string reason) {
                    ISmtpConnector connector = null;
                    try {
                        connector = new SmtpConnector(server, port, false);
                    } catch (Exception ex) {
                        reason = ex.Message;
                        return false;
                    }
                    return DoStartTLScheck(connector, out reason);
                }

                /// <summary>
                ///     Connect to a mail server, send an EHLO command an check the STARTTLS capability. Return true if all is ok.
                ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
                /// </summary>
                public static async Task<bool> StartTLScheckAsync(string server, int port) {
                    return (await StartTLScheckExAsync(server, port).ConfigureAwait(false)).IsSuccess;
                }

                /// <summary>
                ///     Connect to a mail server, send an EHLO command, check and activate the STARTTLS capability. Return the server's reply.
                ///     See https://www.greenend.org.uk/rjk/tech/smtpreplies.html
                /// </summary>
                public static Task<(bool IsSuccess, string Reason)> StartTLScheckExAsync(string server, int port) {
                    ISmtpConnector connector = null;
                    try {
                        connector = new SmtpConnector(server, port, false);
                    } catch (Exception ex) {
                        return Task.FromResult((false, ex.Message));
                    }

                    return DoStartTlsCheckExAsync(connector);
                }
        */

        /// <summary>
        ///     Validate passed credential against an SMTP server. Return "true", if all ok.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="options"></param>
        public static bool ValidateCredentials(string login, string password, string server, int port, SecureSocketMode options) {
            return ValidateCredentials(login, password, server, port, options, out _);
        }

        /// <summary>
        ///     Validate passed credential against an SMTP server. Return "true", if all ok.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="secureMode"></param>
        /// <param name="reason">Server return info</param>
        public static bool ValidateCredentials(string login, string password, string server, int port, SecureSocketMode secureMode, out string reason) {
            ISmtpConnector connector;
            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(server, port, false);
                        if (!DoEHLOcheck(connector, out reason)) return false;
                        break;
                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(server, port, true);
                        connector.AuthenticateAsClient();
                        if (!DoEHLOcheck(connector, out reason)) return false;
                        break;
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(server, port, false);
                        if (!DoStartTlsCheck(connector, out reason)) return false;
                        break;
                    default:
                        throw new NotSupportedException(secureMode.ToString());
                }
            } catch (Exception ex) {
                reason = ex.Message;
                return false;
            }

            //if (!DoEHLOcheck(connector, out reason)) return false;

            connector.SendData($"AUTH LOGIN{SmtpConnector.EOF}");
            if (!connector.CheckResponse(334, out reason)) {
                return false;
            }

            connector.SendData(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}")) + SmtpConnector.EOF);
            if (!connector.CheckResponse(334, out reason)) {
                return false;
            }

            connector.SendData(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{password}")) + SmtpConnector.EOF);
            return connector.CheckResponse(235, out reason);
        }

        /// <summary>
        ///     Validate passed credential against an SMTP server. Return "true", if all ok.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="secureMode"></param>
        public static async Task<bool> ValidateCredentialsAsync(string login, string password, string server, int port, SecureSocketMode secureMode) {
            return (await ValidateCredentialsExAsync(login, password, server, port, secureMode).ConfigureAwait(false)).IsSuccess;
        }

        /// <summary>
        ///     Validate passed credential against an SMTP server. Return "true", if all ok.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="secureMode"></param>
        public static async Task<(bool IsSuccess, string Reason)> ValidateCredentialsExAsync(string login, string password, string server, int port, SecureSocketMode secureMode) {
            ISmtpConnector? connector = null;
            (bool IsSuccess, string Reason) output = (false, string.Empty);

            try {
                switch (secureMode) {
                    case SecureSocketMode.None:
                        connector = new SmtpConnector(smtpServerAddress: server, port: port, enableSsl: false);
                        output = await DoEHLOcheckAsync(connector);
                        if (!output.IsSuccess) return output;
                        break;
                    case SecureSocketMode.SslOnConnect:
                        connector = new SmtpConnector(smtpServerAddress: server, port: port, enableSsl: true);
                        await connector.AuthenticateAsClientAsync();
                        output = await DoEHLOcheckAsync(connector);
                        if (!output.IsSuccess) return output;
                        break;
                    case SecureSocketMode.StartTls:
                        connector = new SmtpConnector(smtpServerAddress: server, port: port, enableSsl: false);
                        output = await DoStartTlsCheckExAsync(connector);
                        if (!output.IsSuccess) return output;
                        break;
                    default:
                        throw new NotSupportedException(secureMode.ToString());
                }
            } catch (Exception ex) {
                return (false, ex.Message);
            }

            //output = await DoEHLOcheckAsync(connector);
            //if (!output.IsSuccess) return output;

            await connector.SendDataAsync($"AUTH LOGIN{SmtpConnector.EOF}").ConfigureAwait(false);
            output = await connector.CheckResponseExAsync(334).ConfigureAwait(false);
            if (!output.IsSuccess) return output;

            await connector.SendDataAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}")) + SmtpConnector.EOF).ConfigureAwait(false);
            output = await connector.CheckResponseExAsync(334).ConfigureAwait(false);
            if (!output.IsSuccess) return output;

            await connector.SendDataAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{password}")) + SmtpConnector.EOF).ConfigureAwait(false);
            return await connector.CheckResponseExAsync(235).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private static bool DoEHLOcheck(ISmtpConnector connector, out string reason) {
            if (!connector.CheckResponse(220, out reason)) return false;

            connector.SendData($"EHLO {Dns.GetHostName()}{SmtpConnector.EOF}");
            return connector.CheckResponse(250, out reason);
        }

        /// <summary>
        ///
        /// </summary>
        private static async Task<(bool IsSuccess, string Reason)> DoEHLOcheckAsync(ISmtpConnector connector) {
            var output = await connector.CheckResponseExAsync(220).ConfigureAwait(false);
            if (!output.IsSuccess) return output;

            await connector.SendDataAsync($"EHLO {Dns.GetHostName()}{SmtpConnector.EOF}").ConfigureAwait(false);
            return await connector.CheckResponseExAsync(250).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private static bool DoStartTlsCheck(ISmtpConnector connector, out string reason) {
            if (!DoEHLOcheck(connector, out reason)) return false;

            //SmtpCapabilities capabilities = CheckSmtpCapabilities(output.responseData);
            //if ((capabilities & SmtpCapabilities.StartTLS) == 0)
            //    return (false, $"STARTTLS is not supported by the SMTP server. Server response was:\n:{output.responseData}");

            connector.SendData($"STARTTLS{SmtpConnector.EOF}");
            if (!connector.CheckResponse(220, out reason)) return false;

            try {
                connector.UpgradeToSsl();
            } catch (Exception ex) {
                reason = $"Cannot upgrade connection to TLS: {ex.Message}";
                return false;
            }

            connector.SendData($"EHLO {Dns.GetHostName()}{SmtpConnector.EOF}");
            var isSuccess = connector.CheckResponse(250, out reason);
            reason = isSuccess
                ? $"Upgrade to TLS is successfully. Server replied to new EHLO with: {reason}"
                : $"Upgrade to TLS was successfully but server replied to new EHLO with: {reason}";
            return isSuccess;
        }

        /// <summary>
        /// 
        /// </summary>
        private static async Task<(bool IsSuccess, string Reason)> DoStartTlsCheckExAsync(ISmtpConnector connector) {
            var output = await DoEHLOcheckAsync(connector);
            if (!output.IsSuccess) return output;

            //SmtpCapabilities capabilities = CheckSmtpCapabilities(output.responseData);
            //if ((capabilities & SmtpCapabilities.StartTLS) == 0)
            //    return (false, $"STARTTLS is not supported by the SMTP server. Server response was:\n:{output.responseData}");

            await connector.SendDataAsync($"STARTTLS{SmtpConnector.EOF}").ConfigureAwait(false);
            output = await connector.CheckResponseExAsync(220).ConfigureAwait(false);
            if (!output.IsSuccess) return output;

            try {
                await connector.UpgradeToSslAsync();
            } catch (Exception ex) {
                return (false, $"Cannot upgrade connection to TLS: {ex.Message}");
            }

            // do again the greatings
            await connector.SendDataAsync($"EHLO {Dns.GetHostName()}{SmtpConnector.EOF}").ConfigureAwait(false);
            output = await connector.CheckResponseExAsync(250).ConfigureAwait(false);
            return (
                IsSuccess: output.IsSuccess,
                Reason: output.IsSuccess
                            ? $"Upgrade to TLS is successfully. Server replied to new EHLO with: {output.Reason}"
                            : $"Upgrade to TLS was successfully but server replied to new EHLO with: {output.Reason}");
        }

        private static SmtpCapabilities CheckSmtpCapabilities(string responseData) {
            SmtpCapabilities capabilities = SmtpCapabilities.None;

            var lines = responseData.Split('\n');
            for (int i = 0; i < lines.Length; i++) {
                // Outlook.com replies with "250-8bitmime" instead of "250-8BITMIME" (strangely, it correctly capitalizes all other extensions...)
                var capability = lines[i].Trim().ToUpperInvariant();

                if (capability.StartsWith("AUTH", StringComparison.Ordinal) || capability.StartsWith("X-EXPS", StringComparison.Ordinal)) {
                    int index = capability[0] == 'A' ? "AUTH".Length : "X-EXPS".Length;

                    if (index < capability.Length && (capability[index] == ' ' || capability[index] == '=')) {
                        capabilities |= SmtpCapabilities.Authentication;
                        index++;

                        // TODO: queste linee?
                        //var mechanisms = capability.Substring(index);
                        //foreach (var mechanism in mechanisms.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        //    AuthenticationMechanisms.Add(mechanism);
                    }
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
            }   // for ...

            return capabilities;
        }

    }
}
