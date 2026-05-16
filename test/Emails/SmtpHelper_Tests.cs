using GPSoftware.Core.Emails;
using SmtpServer;
using SmtpServer.Authentication;
using SmtpServer.ComponentModel;

namespace GPSoftware.Core.Tests.Emails {

    public class SmtpHelper_Tests : IDisposable {

        private readonly SmtpServer.SmtpServer _server;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _serverTask;
        private const int TEST_PORT = 9026; // Different port than SmtpConnector_Tests to avoid conflicts

        public SmtpHelper_Tests() {
            // 1. Configure the Mock SMTP Server
            // We enable Authentication to test ValidateCredentials
            var options = new SmtpServerOptionsBuilder()
                            .ServerName("localhost")
                            .Endpoint(builder =>
                                builder
                                    .Port(TEST_PORT, isSecure: false)
                                    .AllowUnsecureAuthentication() // Correct place for this setting
                            )
                            .Build();

            // 2. Setup a simple Authenticator to validate user/password
            var serviceProvider = new ServiceProvider();
            serviceProvider.Add(new SimpleAuthenticator());

            _server = new SmtpServer.SmtpServer(options, serviceProvider);
            _cancellationTokenSource = new CancellationTokenSource();

            // 3. Start the server
            _serverTask = _server.StartAsync(_cancellationTokenSource.Token);
        }

        public void Dispose() {
            // Gracefully stop the server after tests
            _cancellationTokenSource.Cancel();
            try {
                _serverTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch {
                // Ignore task cancellation exceptions
            }
            _cancellationTokenSource.Dispose();
        }

        // =======================================================================
        // SYNCHRONOUS TESTS
        // =======================================================================

        [Fact]
        public void EHLOcheck_ReturnsTrue_OnValidServer() {
            // Act
            bool result = SmtpHelper.EHLOcheck("127.0.0.1", TEST_PORT, SecureSocketMode.None, out string reason);

            // Assert
            result.ShouldBe(true, reason);
            reason.ShouldContain("250");
        }

        [Fact]
        public void EHLOcheck_ReturnsFalse_OnInvalidPort() {
            // Act
            bool result = SmtpHelper.EHLOcheck("127.0.0.1", 12345, SecureSocketMode.None, out string reason);

            // Assert
            result.ShouldBe(false);
            reason.ShouldNotBeNullOrWhiteSpace(); // Should contain the exception message
        }

        [Fact]
        public void ValidateCredentials_ReturnsTrue_WithCorrectCredentials() {
            // Act
            bool result = SmtpHelper.ValidateCredentials("testuser", "testpass", "127.0.0.1", TEST_PORT, SecureSocketMode.None, out string reason);

            // Assert
            result.ShouldBe(true, reason);
            reason.ShouldContain("235"); // 235 Authentication successful
        }

        [Fact]
        public void ValidateCredentials_ReturnsFalse_WithWrongPassword() {
            // Act
            bool result = SmtpHelper.ValidateCredentials("testuser", "WRONGPASS", "127.0.0.1", TEST_PORT, SecureSocketMode.None, out string reason);

            // Assert
            result.ShouldBe(false, reason);
            // SmtpServer usually returns "535 Authentication credentials invalid"
            reason.ShouldContain("535");
        }

        // =======================================================================
        // ASYNCHRONOUS TESTS (NEW)
        // =======================================================================

        [Fact]
        public async Task EHLOcheckAsync_ReturnsTrue_OnValidServer() {
            // Act
            bool result = await SmtpHelper.EHLOcheckAsync("127.0.0.1", TEST_PORT, SecureSocketMode.None);

            // Assert
            result.ShouldBe(true);
        }

        [Fact]
        public async Task EHLOcheckExAsync_ReturnsTrueAndReason_OnValidServer() {
            // Act
            var result = await SmtpHelper.EHLOcheckExAsync("127.0.0.1", TEST_PORT, SecureSocketMode.None);

            // Assert
            result.IsSuccess.ShouldBe(true, result.Reason);
            result.Reason.ShouldContain("250");
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ReturnsTrue_WithCorrectCredentials() {
            // Act
            bool result = await SmtpHelper.ValidateCredentialsAsync("testuser", "testpass", "127.0.0.1", TEST_PORT, SecureSocketMode.None);

            // Assert
            result.ShouldBe(true);
        }

        [Fact]
        public async Task ValidateCredentialsAsync_ReturnsFalse_WithWrongPassword() {
            // Act
            bool result = await SmtpHelper.ValidateCredentialsAsync("testuser", "WRONGPASS", "127.0.0.1", TEST_PORT, SecureSocketMode.None);

            // Assert
            result.ShouldBe(false);
        }

        [Fact]
        public async Task RawPingSmtpServerAsync_ReturnsTrue_OnValidServer() {
            // Act (Ping with a 2 seconds timeout)
            bool result = await SmtpHelper.RawPingSmtpServerAsync("127.0.0.1", TEST_PORT, 2000);

            // Assert
            result.ShouldBe(true);
        }

        [Fact]
        public async Task RawPingSmtpServerAsync_ReturnsFalse_OnInvalidServer() {
            // Act (Ping an invalid port with a 100ms timeout)
            bool result = await SmtpHelper.RawPingSmtpServerAsync("127.0.0.1", 12345, 100);

            // Assert
            result.ShouldBe(false);
        }

        // =======================================================================
        // MOCK AUTHENTICATOR
        // =======================================================================

        /// <summary>
        /// Simple authenticator class for the Mock Server.
        /// It accepts credentials if user is "testuser" and password is "testpass".
        /// </summary>
        public class SimpleAuthenticator : IUserAuthenticator {
            public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken) {
                if (user == "testuser" && password == "testpass") {
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
        }
    }
}
