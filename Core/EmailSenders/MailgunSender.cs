using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public class MailgunSender : IEmailSender
    {
        private string _hostEmail;
        private string _apiKey;
        private static readonly HttpClient _httpClient = new HttpClient();

        public MailgunSender(string apiKey, string hostEmail)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException("apikey is null", nameof(apiKey));
            }

            if (string.IsNullOrEmpty(hostEmail))
            {
                throw new ArgumentNullException("hostemail is null", nameof(hostEmail));
            }

            _hostEmail = hostEmail;
            _apiKey = apiKey;
        }

        public async Task SendEmailWithHtmlAttachmentAsync(string email, string subject, string htmlContent)
        {
            var domainName = _hostEmail.Split('@')[1];
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.eu.mailgun.net/v3/{domainName}/messages");

            var fileContent = new ByteArrayContent(Encoding.ASCII.GetBytes(htmlContent));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");

            var authByteArray = Encoding.ASCII.GetBytes($"api:{_apiKey}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authByteArray));

            request.Content = new MultipartFormDataContent
            {
                {new StringContent("kindle@mail.p2k.sejka.pl"), "from" },
                {new StringContent(email), "to" },
                {new StringContent(subject), "subject" },
                {new StringContent("sent from pocket to kindle app"), "text" },
                {fileContent, "attachment", $"{subject.Normalize()}.html" }
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send email: {email}, {subject}, {await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}