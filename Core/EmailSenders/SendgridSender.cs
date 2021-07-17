using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public class SendgridSender : IEmailSender
    {
        private string _hostEmail;
        private string _apiKey;
        private static readonly HttpClient _httpClient = new HttpClient();

        public SendgridSender(string apiKey, string hostEmail)
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
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.sendgrid.com/v3/mail/send");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            request.Content = new StringContent("{\"personalizations\": [{\"to\": [{\"email\": \"" + email + "\"}]}],\"from\": {\"email\": \"" + _hostEmail + "\"},\"subject\":\"" + subject + "\",\"content\": [{\"type\": \"text/html\",\"value\": \"p2k.sejka.pl\"}], \"attachments\": [{\"content\": \"" + Base64Encode(htmlContent) + "\", \"type\": \"text/plain\", \"filename\": \"" + subject.Normalize() + ".html\"}]}", Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send email: {email}, {subject}, {await response.Content.ReadAsStringAsync()}");
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}