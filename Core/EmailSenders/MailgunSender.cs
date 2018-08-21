using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Core.EmailSenders
{
    public class MailgunSender : IEmailSender
    {
        private string _hostEmail;
        private HttpClient _httpClient = new HttpClient();

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

            var authByteArray = Encoding.ASCII.GetBytes($"api:{apiKey}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authByteArray));
            _hostEmail = hostEmail;
            var domainName = hostEmail.Split('@')[1];

            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("invalid hostemail", nameof(domainName));
            }

            _httpClient.BaseAddress = new Uri($"https://api.eu.mailgun.net/v3/{domainName}/messages");
        }

        public async Task SendEmailWithHtmlAttachmentAsync(string email, string subject, string htmlContent)
        {
            var parameters = new Dictionary<string, string> {
                { "from", _hostEmail },
                { "to", email },
                { "subject", subject },
                { "text", "sent from pocket to kindle app" }
            };

            var request = new MultipartFormDataContent();

            foreach (var parameter in parameters)
            {
                request.Add(new StringContent(parameter.Value), parameter.Key);
            }

            var fileContent = new ByteArrayContent(Encoding.ASCII.GetBytes(htmlContent));
            fileContent.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "attachment",
                        FileName = $"{subject.Normalize()}.html",
                    };
            request.Add(fileContent);

            var response = await _httpClient.PostAsync("", request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to send email: {email}, {subject}, {response.Content}");
            }
        }
    }
}