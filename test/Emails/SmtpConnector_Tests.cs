using GPSoftware.Core.Emails;
using SmtpServer;
using SmtpServer.ComponentModel;

namespace GPSoftware.Core.Tests.Emails {

    public class SmtpConnector_Tests : IDisposable {

        private readonly SmtpServer.SmtpServer _server;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _serverTask;
        private const int TEST_PORT = 9025; // High port to avoid permission issues

        public SmtpConnector_Tests() {
            // 1. Configure the Mock SMTP Server using the SmtpServer library
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(TEST_PORT)
                .Build();

            // Minimal service provider setup
            var serviceProvider = new ServiceProvider();

            _server = new SmtpServer.SmtpServer(options, serviceProvider);
            _cancellationTokenSource = new CancellationTokenSource();

            // 2. Start the server in background (Fire and Forget)
            _serverTask = _server.StartAsync(_cancellationTokenSource.Token);
        }

        public void Dispose() {
            // Gracefully stop the server after tests
            _cancellationTokenSource.Cancel();
            try {
                _serverTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch {
                // Ignore task cancellation exceptions during teardown
            }
            _cancellationTokenSource.Dispose();
        }

        [Fact]
        public void Constructor_ConnectsSuccessfully_ToRealLibrary() {
            // Arrange & Act
            using var connector = new SmtpConnector("127.0.0.1", TEST_PORT, enableSsl: false);

            // Assert
            Assert.NotNull(connector);
            Assert.Equal("127.0.0.1", connector.SmtpServerAddress);
        }

        [Fact]
        public void CheckResponse_ReturnsTrue_OnInitialConnection() {
            // Upon connection, a real SMTP server sends "220 Service ready"
            using var connector = new SmtpConnector("127.0.0.1", TEST_PORT, false);

            // Act
            // We verify that our connector can parse the standard 220 code
            bool result = connector.CheckResponse(220, out string response);

            // Assert
            Assert.True(result);
            Assert.Contains("220", response); // The SmtpServer library replies with a standard banner
        }

        [Fact]
        public async Task SendData_InteractsWithRealServer_Async() {
            using var connector = new SmtpConnector("127.0.0.1", TEST_PORT, false);

            // Consume the welcome message (220) before sending commands
            await connector.CheckResponseAsync(220);

            // Act: Send a standard HELO/EHLO command
            await connector.SendDataAsync("EHLO gpsoftware.test\r\n");

            // Assert: The real server MUST reply with "250 OK"
            var result = await connector.CheckResponseExAsync(250);

            Assert.True(result.IsSuccess, $"Server replied: {result.ResponseData}");
        }

        [Fact]
        public void CheckResponse_Fails_OnProtocolMismatch() {
            // Robustness test: checking for a code that the server did NOT send
            using var connector = new SmtpConnector("127.0.0.1", TEST_PORT, false);

            // The server sends 220, we ask if it sent 500
            bool result = connector.CheckResponse(500, out string response);

            // Assert
            Assert.False(result);
            Assert.Contains("220", response); // Verify what was actually received
        }
    }
}
