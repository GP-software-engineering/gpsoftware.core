using System.Threading.Tasks;

namespace GPSoftware.Core.Emails {

    public interface ISmtpConnector {

        bool CheckResponse(int expectedCode, out string responseData);

        bool CheckResponse(int[] expectedCodes, out string responseData);

        bool CheckResponse(params int[] expectedCodes);

        Task<bool> CheckResponseAsync(params int[] expectedCodes);

        Task<(bool IsSuccess, string ResponseData)> CheckResponseExAsync(params int[] expectedCodes);

        void SendData(string data);

        Task SendDataAsync(string data);

        void AuthenticateAsClient(bool checkCertificateRevocation = false);

        Task AuthenticateAsClientAsync(bool checkCertificateRevocation = false);

        void UpgradeToSsl(bool checkCertificateRevocation = false);

        Task UpgradeToSslAsync(bool checkCertificateRevocation = false);
    }
}